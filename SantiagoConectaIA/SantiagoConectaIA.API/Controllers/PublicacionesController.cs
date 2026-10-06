using Microsoft.AspNetCore.Mvc;
using EngramaCoreStandar.Results;
using SantiagoConectaIA.API.EngramaLevels.Domain.Interfaces;
using SantiagoConectaIA.API.Services;
using SantiagoConectaIA.Share.PostModels.NoticiasModule;
using SantiagoConectaIA.Share.PostModels.PublicacionesModule;

namespace SantiagoConectaIA.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PublicacionesController : ControllerBase
    {
        private readonly IFacebookPublishService _facebookPublishService;
        private readonly IGeminiPublicacionService _geminiPublicacionService;
        private readonly IGeminiImagenService _geminiImagenService;
        private readonly IPublicacionAutomaticaService _publicacionAutomaticaService;
        private readonly IEmprendimientoAutomaticoService _emprendimientoAutomaticoService;
        private readonly IParametrosService _parametros;
        private readonly INoticiasDomain _noticiasDomain;
        private readonly PublicacionesBitacora _bitacora;
        private readonly INoticiaAutomaticaService _noticiaAutomaticaService;

        public PublicacionesController(
            IFacebookPublishService facebookPublishService,
            IGeminiPublicacionService geminiPublicacionService,
            IGeminiImagenService geminiImagenService,
            IPublicacionAutomaticaService publicacionAutomaticaService,
            IEmprendimientoAutomaticoService emprendimientoAutomaticoService,
            IParametrosService parametros,
            INoticiasDomain noticiasDomain,
            PublicacionesBitacora bitacora,
            INoticiaAutomaticaService noticiaAutomaticaService)
        {
            _facebookPublishService = facebookPublishService;
            _geminiPublicacionService = geminiPublicacionService;
            _geminiImagenService = geminiImagenService;
            _publicacionAutomaticaService = publicacionAutomaticaService;
            _emprendimientoAutomaticoService = emprendimientoAutomaticoService;
            _parametros = parametros;
            _noticiasDomain = noticiasDomain;
            _bitacora = bitacora;
            _noticiaAutomaticaService = noticiaAutomaticaService;
        }

        /// <summary>
        /// Pide a Gemini un texto de Facebook a partir del título y el contenido de la noticia.
        /// </summary>
        [HttpPost("PostMejorarPublicacion")]
        public async Task<IActionResult> PostMejorarPublicacion([FromBody] PostMejorarPublicacion postModel, CancellationToken cancellationToken)
        {
            var result = await _geminiPublicacionService.MejorarAsync(postModel, cancellationToken);
            if (result.IsSuccess)
            {
                return Ok(result);
            }

            return BadRequest(result);
        }

        /// <summary>
        /// Pide a Gemini un texto de Facebook que presente un emprendimiento, sus productos y dónde encontrarlo.
        /// </summary>
        [HttpPost("PostMejorarEmprendimiento")]
        public async Task<IActionResult> PostMejorarEmprendimiento([FromBody] PostMejorarEmprendimiento postModel, CancellationToken cancellationToken)
        {
            var result = await _geminiPublicacionService.MejorarEmprendimientoAsync(postModel, cancellationToken);
            if (result.IsSuccess)
            {
                return Ok(result);
            }

            return BadRequest(result);
        }

        /// <summary>
        /// Pide a Gemini un texto de Facebook que promocione un producto de un emprendimiento.
        /// </summary>
        [HttpPost("PostMejorarProducto")]
        public async Task<IActionResult> PostMejorarProducto([FromBody] PostMejorarProducto postModel, CancellationToken cancellationToken)
        {
            var result = await _geminiPublicacionService.MejorarProductoAsync(postModel, cancellationToken);
            if (result.IsSuccess)
            {
                return Ok(result);
            }

            return BadRequest(result);
        }

        /// <summary>
        /// Edita una imagen con Gemini según el prompt, la guarda en Azure Blob y devuelve su URL.
        /// </summary>
        [HttpPost("PostEditarImagenIa")]
        public async Task<IActionResult> PostEditarImagenIa([FromBody] PostEditarImagen postModel, CancellationToken cancellationToken)
        {
            var result = await _geminiImagenService.EditarAsync(postModel, cancellationToken);
            if (result.IsSuccess)
            {
                return Ok(result);
            }

            return BadRequest(result);
        }

        /// <summary>
        /// Envía el texto y la imagen al webhook de Make para publicarlos en Facebook.
        /// </summary>
        [HttpPost("PostPublicarFacebook")]
        public async Task<IActionResult> PostPublicarFacebook([FromBody] PostPublicarFacebook postModel, CancellationToken cancellationToken)
        {
            var result = await _facebookPublishService.PublicarAsync(postModel, cancellationToken);
            if (result.IsSuccess)
            {
                return Ok(result);
            }

            return BadRequest(result);
        }

        /// <summary>
        /// Consulta si la publicación diaria automática de noticias está activa.
        /// </summary>
        [HttpPost("PostGetPublicacionAutomatica")]
        public async Task<IActionResult> PostGetPublicacionAutomatica(CancellationToken cancellationToken)
        {
            var estado = await _publicacionAutomaticaService.GetAsync(cancellationToken);
            return Ok(new Response<PublicacionAutomatica> { IsSuccess = true, Data = estado, Message = "Ok" });
        }

        /// <summary>
        /// Activa o desactiva la publicación diaria automática de noticias.
        /// </summary>
        [HttpPost("PostSavePublicacionAutomatica")]
        public async Task<IActionResult> PostSavePublicacionAutomatica([FromBody] PublicacionAutomatica postModel, CancellationToken cancellationToken)
        {
            var estado = await _publicacionAutomaticaService.SetActivoAsync(postModel?.bActivo ?? false, cancellationToken);
            var mensaje = estado.bActivo ? "Publicación automática activada." : "Publicación automática desactivada.";
            return Ok(new Response<PublicacionAutomatica> { IsSuccess = true, Data = estado, Message = mensaje });
        }

        /// <summary>
        /// Consulta el estado de la publicación diaria automática de emprendimientos y cuál sigue.
        /// </summary>
        [HttpPost("PostGetPublicacionAutomaticaEmprendimientos")]
        public async Task<IActionResult> PostGetPublicacionAutomaticaEmprendimientos(CancellationToken cancellationToken)
        {
            var estado = await EstadoEmprendimientosAsync(await _publicacionAutomaticaService.GetEmprendimientosAsync(cancellationToken), cancellationToken);
            return Ok(new Response<PublicacionAutomaticaEmprendimientos> { IsSuccess = true, Data = estado, Message = "Ok" });
        }

        /// <summary>
        /// Activa o desactiva la publicación diaria automática de emprendimientos.
        /// </summary>
        [HttpPost("PostSavePublicacionAutomaticaEmprendimientos")]
        public async Task<IActionResult> PostSavePublicacionAutomaticaEmprendimientos([FromBody] PublicacionAutomaticaEmprendimientos postModel, CancellationToken cancellationToken)
        {
            var guardado = await _publicacionAutomaticaService.SetEmprendimientosActivoAsync(postModel?.bActivo ?? false, cancellationToken);
            var estado = await EstadoEmprendimientosAsync(guardado, cancellationToken);
            var mensaje = estado.bActivo ? "Publicación automática de emprendimientos activada." : "Publicación automática de emprendimientos desactivada.";
            return Ok(new Response<PublicacionAutomaticaEmprendimientos> { IsSuccess = true, Data = estado, Message = mensaje });
        }

        /// <summary>
        /// Publica ahora la noticia activa más reciente, siempre que no se haya publicado ya.
        /// </summary>
        [HttpPost("PostPublicarSiguienteNoticia")]
        public async Task<IActionResult> PostPublicarSiguienteNoticia(CancellationToken cancellationToken)
        {
            var resultado = await _noticiaAutomaticaService.PublicarMasRecienteAsync(manual: true, cancellationToken);
            var respuesta = new Response<PublicacionAutomatica>
            {
                IsSuccess = resultado.IsSuccess,
                Message = resultado.Message,
                Data = await _publicacionAutomaticaService.GetAsync(cancellationToken)
            };

            return resultado.IsSuccess ? Ok(respuesta) : BadRequest(respuesta);
        }

        /// <summary>
        /// Publica ahora el siguiente emprendimiento de la rotación y avanza el puntero al que sigue.
        /// </summary>
        [HttpPost("PostPublicarSiguienteEmprendimiento")]
        public async Task<IActionResult> PostPublicarSiguienteEmprendimiento(CancellationToken cancellationToken)
        {
            var resultado = await _emprendimientoAutomaticoService.PublicarSiguienteAsync(manual: true, cancellationToken);
            var estado = await EstadoEmprendimientosAsync(await _publicacionAutomaticaService.GetEmprendimientosAsync(cancellationToken), cancellationToken);
            var respuesta = new Response<PublicacionAutomaticaEmprendimientos>
            {
                IsSuccess = resultado.IsSuccess,
                Message = resultado.Message,
                Data = estado
            };

            return resultado.IsSuccess ? Ok(respuesta) : BadRequest(respuesta);
        }
        /// <summary>
        /// Revisa, sin publicar nada, si las publicaciones automáticas pueden funcionar: parámetros,
        /// siguiente noticia y emprendimiento, horarios y resultado del último intento.
        /// Solo devuelve si algo está configurado; nunca los valores.
        /// </summary>
        [HttpGet("Diagnostico")]
        public async Task<IActionResult> Diagnostico(CancellationToken cancellationToken)
        {
            var zona = HorarioPublicacion.ZonaHoraria();
            DateTime? Local(DateTime? utc) => utc.HasValue ? TimeZoneInfo.ConvertTimeFromUtc(DateTime.SpecifyKind(utc.Value, DateTimeKind.Utc), zona) : null;

            var webhook = await _parametros.GetValor1Async(ParametrosAlias.MakeFacebookWebhook, cancellationToken);
            var claveGemini = await _parametros.GetValor2Async(ParametrosAlias.Gemini, cancellationToken);
            var modeloGemini = await _parametros.GetValor1Async(ParametrosAlias.Gemini, cancellationToken);
            var sitio = await _parametros.GetValor1Async(ParametrosAlias.PublicacionesSitio, cancellationToken);
            var horaNoticias = await _publicacionAutomaticaService.HoraPublicacionAsync(cancellationToken);
            var horaEmprendimientos = await _publicacionAutomaticaService.HoraEmprendimientosAsync(cancellationToken);

            var estadoNoticias = await _publicacionAutomaticaService.GetAsync(cancellationToken);
            var estadoEmprendimientos = await _publicacionAutomaticaService.GetEmprendimientosAsync(cancellationToken);
            var siguienteEmpresa = await _emprendimientoAutomaticoService.GetSiguienteAsync(estadoEmprendimientos.iIdSiguienteEmpresa, cancellationToken);

            object? noticia = null;
            var noticiaSinImagen = false;
            var respuesta = await _noticiasDomain.GetNoticias(new PostGetNoticias { bActivo = true });
            if (respuesta.IsSuccess && respuesta.Data != null)
            {
                var candidata = respuesta.Data.OrderByDescending(n => n.dtFechaPublicacion).ThenByDescending(n => n.iIdNoticia).FirstOrDefault();
                if (candidata != null)
                {
                    noticiaSinImagen = string.IsNullOrWhiteSpace(candidata.vchImagenPortada);
                    noticia = new
                    {
                        candidata.iIdNoticia,
                        titulo = candidata.vchTitulo,
                        fecha = candidata.dtFechaPublicacion,
                        tieneImagen = !string.IsNullOrWhiteSpace(candidata.vchImagenPortada),
                        yaPublicada = estadoNoticias.iIdUltimaNoticia == candidata.iIdNoticia
                    };
                }
            }

            object Proceso(string nombre, bool activo, int hora, object? siguiente)
            {
                var intento = _bitacora.UltimoIntento(nombre);
                var proxima = _bitacora.ProximaEjecucionUtc(nombre);
                return new
                {
                    activo,
                    horaConfigurada = hora,
                    programadaLocal = Local(proxima),
                    ultimoIntentoLocal = Local(intento?.Utc),
                    ultimoResultado = intento?.Resultado ?? "Sin intentos desde que arrancó el servidor.",
                    siguiente
                };
            }

            var ahoraUtc = DateTime.UtcNow;
            var servidorUtc = _bitacora.InicioUtc;
            var problemas = new List<string>();
            if (string.IsNullOrWhiteSpace(claveGemini)) problemas.Add($"Falta la clave de Gemini (parámetro {ParametrosAlias.Gemini}, Valor2).");
            if (string.IsNullOrWhiteSpace(webhook)) problemas.Add($"Falta la URL del webhook de Make (parámetro {ParametrosAlias.MakeFacebookWebhook}, Valor1).");
            else if (!webhook.StartsWith("https://", StringComparison.OrdinalIgnoreCase)) problemas.Add("El webhook de Make no empieza con https://.");
            if (_bitacora.ProximaEjecucionUtc(PublicacionesBitacora.Noticias) == null || _bitacora.ProximaEjecucionUtc(PublicacionesBitacora.Emprendimientos) == null)
                problemas.Add("Los servicios en segundo plano no han quedado programados: no están corriendo.");
            if (ahoraUtc - servidorUtc < TimeSpan.FromHours(6))
                problemas.Add($"El servidor lleva encendido solo {(ahoraUtc - servidorUtc).TotalMinutes:0} minutos. Si se apaga por inactividad (falta 'Always On'), no llega a la hora programada.");
            if (noticiaSinImagen)
                problemas.Add("La noticia más reciente no tiene imagen de portada: no se puede publicar.");

            return Ok(new
            {
                problemas,
                servidor = new
                {
                    ahoraLocal = Local(ahoraUtc),
                    zonaHoraria = zona.Id,
                    arranqueLocal = Local(servidorUtc),
                    minutosEncendido = (int)(ahoraUtc - servidorUtc).TotalMinutes
                },
                parametros = new
                {
                    geminiClave = !string.IsNullOrWhiteSpace(claveGemini),
                    geminiModelo = modeloGemini,
                    makeWebhook = !string.IsNullOrWhiteSpace(webhook),
                    sitioPublico = sitio
                },
                noticias = Proceso(PublicacionesBitacora.Noticias, estadoNoticias.bActivo, horaNoticias, noticia),
                emprendimientos = Proceso(PublicacionesBitacora.Emprendimientos, estadoEmprendimientos.bActivo, horaEmprendimientos,
                    siguienteEmpresa == null ? null : new { siguienteEmpresa.iIdEmpresa, nombre = siguienteEmpresa.vchNombreComercial })
            });
        }
        private async Task<PublicacionAutomaticaEmprendimientos> EstadoEmprendimientosAsync(PublicacionAutomaticaEmprendimientos estado, CancellationToken cancellationToken)
        {
            var siguiente = await _emprendimientoAutomaticoService.GetSiguienteAsync(estado.iIdSiguienteEmpresa, cancellationToken);
            if (siguiente != null)
            {
                estado.iIdSiguienteEmpresa = siguiente.iIdEmpresa;
                estado.vchSiguienteEmpresa = siguiente.vchNombreComercial;
            }

            return estado;
        }
    }
}
