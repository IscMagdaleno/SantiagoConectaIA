using SantiagoConectaIA.API.EngramaLevels.Domain.Interfaces.EmpresasModule;
using SantiagoConectaIA.Share.Objects.EmpresasModulo;
using SantiagoConectaIA.Share.PostClass.EmpresasModulo;
using SantiagoConectaIA.Share.PostModels.EmpresasModulo;
using SantiagoConectaIA.Share.PostModels.PublicacionesModule;
using SantiagoConectaIA.Share.Utilities;

namespace SantiagoConectaIA.API.Services
{
    public class EmprendimientoAutomaticoService : IEmprendimientoAutomaticoService
    {
        private readonly IEmpresasDomain _empresasDomain;
        private readonly IGeminiPublicacionService _gemini;
        private readonly IFacebookPublishService _facebook;
        private readonly IPublicacionAutomaticaService _automatica;
        private readonly ILogger<EmprendimientoAutomaticoService> _logger;

        public EmprendimientoAutomaticoService(
            IEmpresasDomain empresasDomain,
            IGeminiPublicacionService gemini,
            IFacebookPublishService facebook,
            IPublicacionAutomaticaService automatica,
            ILogger<EmprendimientoAutomaticoService> logger)
        {
            _empresasDomain = empresasDomain;
            _gemini = gemini;
            _facebook = facebook;
            _automatica = automatica;
            _logger = logger;
        }

        public async Task<Empresa?> GetSiguienteAsync(int idSiguiente, CancellationToken cancellationToken = default)
        {
            var orden = await OrdenRotacionAsync(idSiguiente);
            return orden.FirstOrDefault(e => TextoPublicacion.EsUrlHttp(e.vchLogoUrl));
        }

        public async Task PublicarSiguienteAsync(CancellationToken cancellationToken = default)
        {
            var estado = await _automatica.GetEmprendimientosAsync(cancellationToken);
            if (!estado.bActivo)
            {
                _logger.LogInformation("La publicación automática de emprendimientos está desactivada. No se publica hoy.");
                return;
            }

            if (estado.dtUltimaPublicacion.HasValue && HorarioPublicacion.FechaLocal(estado.dtUltimaPublicacion.Value) == HorarioPublicacion.HoyLocal())
            {
                _logger.LogInformation("Ya se publicó un emprendimiento hoy ({Nombre}). No se publica otro.", estado.vchUltimaEmpresa);
                return;
            }

            var orden = await OrdenRotacionAsync(estado.iIdSiguienteEmpresa);
            if (!orden.Any())
            {
                _logger.LogInformation("No hay emprendimientos activos para publicar en Facebook.");
                return;
            }

            foreach (var empresa in orden)
            {
                cancellationToken.ThrowIfCancellationRequested();

                if (!TextoPublicacion.EsUrlHttp(empresa.vchLogoUrl))
                {
                    _logger.LogWarning("El emprendimiento {Id} ({Nombre}) no tiene logo. Se pasa al siguiente.", empresa.iIdEmpresa, empresa.vchNombreComercial);
                    continue;
                }

                var detalle = await _empresasDomain.GetEmprendimientoFullById(new PostGetEmprendimientoFullById
                {
                    iIdEmpresa = empresa.iIdEmpresa,
                    iIdPropietario = empresa.iIdPropietario ?? 0
                });

                if (!detalle.IsSuccess || detalle.Data == null)
                {
                    _logger.LogWarning("No se pudo leer el emprendimiento {Id}: {Mensaje}. Se pasa al siguiente.", empresa.iIdEmpresa, detalle.Message);
                    continue;
                }

                if (detalle.Data.Empresa == null || detalle.Data.Empresa.iIdEmpresa <= 0)
                {
                    detalle.Data.Empresa = empresa;
                }

                var informacion = EmprendimientoTextoBuilder.ArmarInformacion(detalle.Data);
                var nombre = empresa.vchNombreComercial ?? string.Empty;

                _logger.LogInformation("Redactando con IA el emprendimiento {Id} ({Nombre}).", empresa.iIdEmpresa, nombre);
                var mejorado = await _gemini.MejorarEmprendimientoAsync(new PostMejorarEmprendimiento
                {
                    iIdEmpresa = empresa.iIdEmpresa,
                    vchNombreComercial = nombre,
                    nvchInformacion = informacion
                }, cancellationToken);

                if (!mejorado.IsSuccess || string.IsNullOrWhiteSpace(mejorado.Data))
                {
                    _logger.LogWarning("Gemini no redactó el emprendimiento {Id}: {Mensaje}. Se reintentará en la siguiente ejecución.", empresa.iIdEmpresa, mejorado.Message);
                    return;
                }

                var publicado = await _facebook.PublicarAsync(new PostPublicarFacebook
                {
                    Message = mejorado.Data,
                    ImageUrl = empresa.vchLogoUrl!.Trim()
                }, cancellationToken);

                if (!publicado.IsSuccess)
                {
                    _logger.LogWarning("Make no aceptó el emprendimiento {Id}: {Mensaje}. Se reintentará en la siguiente ejecución.", empresa.iIdEmpresa, publicado.Message);
                    return;
                }

                await _automatica.RegistrarEmprendimientoAsync(empresa.iIdEmpresa, nombre, cancellationToken);
                _logger.LogInformation("Emprendimiento {Id} ({Nombre}) publicado en Facebook.", empresa.iIdEmpresa, nombre);
                return;
            }

            _logger.LogWarning("Ningún emprendimiento activo tiene logo. No se publicó en Facebook.");
        }

        private async Task<List<Empresa>> OrdenRotacionAsync(int idSiguiente)
        {
            var respuesta = await _empresasDomain.GetEmpresas(new PostGetEmpresas { bEstatus = true });
            if (!respuesta.IsSuccess || respuesta.Data == null)
            {
                _logger.LogWarning("No se pudo consultar los emprendimientos: {Mensaje}", respuesta.Message);
                return new List<Empresa>();
            }

            var activos = respuesta.Data
                .Where(e => e.iIdEmpresa > 0 && e.bEstatus)
                .OrderBy(e => e.iIdEmpresa)
                .ToList();

            return activos.Where(e => e.iIdEmpresa >= idSiguiente)
                .Concat(activos.Where(e => e.iIdEmpresa < idSiguiente))
                .ToList();
        }
    }
}
