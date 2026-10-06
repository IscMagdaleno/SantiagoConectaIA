using EngramaCoreStandar.Results;
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
        /// <summary>Evita que la publicación diaria y el botón manual publiquen al mismo tiempo.</summary>
        private static readonly SemaphoreSlim Candado = new(1, 1);

        /// <summary>Protege contra doble clic: no se publica de nuevo en menos de este tiempo.</summary>
        private static readonly TimeSpan EsperaEntrePublicaciones = TimeSpan.FromMinutes(2);

        private readonly IEmpresasDomain _empresasDomain;
        private readonly IGeminiPublicacionService _gemini;
        private readonly IFacebookPublishService _facebook;
        private readonly IPublicacionAutomaticaService _automatica;
        private readonly PublicacionesBitacora _bitacora;
        private readonly ILogger<EmprendimientoAutomaticoService> _logger;

        public EmprendimientoAutomaticoService(
            IEmpresasDomain empresasDomain,
            IGeminiPublicacionService gemini,
            IFacebookPublishService facebook,
            IPublicacionAutomaticaService automatica,
            PublicacionesBitacora bitacora,
            ILogger<EmprendimientoAutomaticoService> logger)
        {
            _empresasDomain = empresasDomain;
            _gemini = gemini;
            _facebook = facebook;
            _automatica = automatica;
            _bitacora = bitacora;
            _logger = logger;
        }

        public async Task<Empresa?> GetSiguienteAsync(int idSiguiente, CancellationToken cancellationToken = default)
        {
            var orden = await OrdenRotacionAsync(idSiguiente);
            return orden.FirstOrDefault(e => TextoPublicacion.EsUrlHttp(e.vchLogoUrl));
        }

        public async Task<Response<string>> PublicarSiguienteAsync(bool manual = false, CancellationToken cancellationToken = default)
        {
            if (!await Candado.WaitAsync(0, cancellationToken))
            {
                return Anotar(Falla("Ya hay una publicación de emprendimientos en curso. Espera a que termine."));
            }

            try
            {
                return Anotar(await PublicarAsync(manual, cancellationToken));
            }
            finally
            {
                Candado.Release();
            }
        }

        private async Task<Response<string>> PublicarAsync(bool manual, CancellationToken cancellationToken)
        {
            var estado = await _automatica.GetEmprendimientosAsync(cancellationToken);

            // El interruptor solo gobierna la publicación diaria; el botón manual siempre puede publicar.
            if (!manual && !estado.bActivo)
            {
                _logger.LogInformation("La publicación automática de emprendimientos está desactivada. No se publica hoy.");
                return Falla("Desactivada: no se publicó.");
            }

            if (!manual && estado.dtUltimaPublicacion.HasValue && HorarioPublicacion.FechaLocal(estado.dtUltimaPublicacion.Value) == HorarioPublicacion.HoyLocal())
            {
                _logger.LogInformation("Ya se publicó un emprendimiento hoy ({Nombre}). No se publica otro.", estado.vchUltimaEmpresa);
                return Falla($"Ya se publicó hoy ({estado.vchUltimaEmpresa}).");
            }

            if (manual && estado.dtUltimaPublicacion.HasValue && DateTime.UtcNow - estado.dtUltimaPublicacion.Value < EsperaEntrePublicaciones)
            {
                return Falla($"Se acaba de publicar {estado.vchUltimaEmpresa}. Espera un par de minutos antes de publicar el siguiente.");
            }

            var orden = await OrdenRotacionAsync(estado.iIdSiguienteEmpresa);
            if (!orden.Any())
            {
                _logger.LogInformation("No hay emprendimientos activos para publicar en Facebook.");
                return Falla("No hay emprendimientos activos.");
            }

            // Con un solo emprendimiento activo, el siguiente sería el que ya se publicó.
            if (orden.Count == 1 && orden[0].iIdEmpresa == estado.iIdUltimaEmpresa)
            {
                return Falla($"Solo hay un emprendimiento activo ({orden[0].vchNombreComercial}) y ya se publicó.");
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
                    return Falla($"Gemini no redactó el emprendimiento {empresa.iIdEmpresa}: {mejorado.Message}");
                }

                var publicado = await _facebook.PublicarAsync(new PostPublicarFacebook
                {
                    Message = mejorado.Data,
                    ImageUrl = empresa.vchLogoUrl!.Trim()
                }, cancellationToken);

                if (!publicado.IsSuccess)
                {
                    _logger.LogWarning("Make no aceptó el emprendimiento {Id}: {Mensaje}. Se reintentará en la siguiente ejecución.", empresa.iIdEmpresa, publicado.Message);
                    return Falla($"Make no aceptó el emprendimiento {empresa.iIdEmpresa}: {publicado.Message}");
                }

                await _automatica.RegistrarEmprendimientoAsync(empresa.iIdEmpresa, nombre, cancellationToken);
                _logger.LogInformation("Emprendimiento {Id} ({Nombre}) publicado en Facebook.", empresa.iIdEmpresa, nombre);
                return Exito($"Emprendimiento #{empresa.iIdEmpresa} ({nombre}) publicado en Facebook.");
            }

            _logger.LogWarning("Ningún emprendimiento activo tiene logo. No se publicó en Facebook.");
            return Falla("Ningún emprendimiento activo tiene logo.");
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

        private Response<string> Anotar(Response<string> resultado)
        {
            _bitacora.Registrar(PublicacionesBitacora.Emprendimientos, resultado.Message);
            return resultado;
        }

        private static Response<string> Exito(string mensaje) => new() { IsSuccess = true, Data = mensaje, Message = mensaje };

        private static Response<string> Falla(string mensaje) => Response<string>.BadResult(mensaje, string.Empty);
    }
}
