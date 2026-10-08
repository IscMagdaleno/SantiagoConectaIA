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
        /// Pide a Gemini un texto de Facebook que promocione un evento.
        /// </summary>
        [HttpPost("PostMejorarEvento")]
        public async Task<IActionResult> PostMejorarEvento([FromBody] PostMejorarEvento postModel, CancellationToken cancellationToken)
        {
            var result = await _geminiPublicacionService.MejorarEventoAsync(postModel, cancellationToken);
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
        /// Guarda una imagen generada en el cliente (Base64) en Azure Blob Storage y devuelve su URL.
        /// </summary>
        [HttpPost("PostGuardarImagenBase64")]
        public async Task<IActionResult> PostGuardarImagenBase64([FromBody] PostGuardarImagenGenerada postModel, [FromServices] IAzureBlobDomain blobDomain, CancellationToken cancellationToken)
        {
            if (postModel == null || string.IsNullOrWhiteSpace(postModel.Base64Data))
            {
                return BadRequest(Response<string>.BadResult("No se proporcionó la imagen en base64.", string.Empty));
            }

            try
            {
                var base64 = postModel.Base64Data;
                var commaIndex = base64.IndexOf(',');
                if (commaIndex >= 0)
                {
                    base64 = base64.Substring(commaIndex + 1);
                }

                var bytes = Convert.FromBase64String(base64);
                var rawTitle = !string.IsNullOrWhiteSpace(postModel.Titulo) ? postModel.Titulo.Trim() : "post";
                var safeTitle = string.Concat(rawTitle.Select(c => char.IsLetterOrDigit(c) ? c : '_'));
                if (safeTitle.Length > 40) safeTitle = safeTitle.Substring(0, 40);

                var fileName = $"post-{safeTitle}-{DateTime.UtcNow:yyyyMMddHHmmss}-{Guid.NewGuid():N}.png";

                using var stream = new MemoryStream(bytes);
                var subida = await blobDomain.UploadDocument(stream, fileName, "publicaciones");

                if (!subida.IsSuccess || string.IsNullOrWhiteSpace(subida.Data?.URL))
                {
                    return BadRequest(Response<string>.BadResult(subida.Message ?? "Error al guardar la imagen en el almacenamiento.", string.Empty));
                }

                return Ok(new Response<string> { IsSuccess = true, Data = subida.Data.URL, Message = "Imagen guardada correctamente." });
            }
            catch (Exception ex)
            {
                return BadRequest(Response<string>.BadResult($"Error al procesar la imagen: {ex.Message}", string.Empty));
            }
        }

        /// <summary>
        /// Descarga una imagen externa por URL desde el servidor y la devuelve en formato Data URL Base64 para evitar errores de CORS en el navegador.
        /// </summary>
        [HttpPost("PostProxyImageBase64")]
        public async Task<IActionResult> PostProxyImageBase64([FromBody] PostEditarImagen postModel, [FromServices] IHttpClientFactory httpClientFactory, CancellationToken cancellationToken)
        {
            if (postModel == null || string.IsNullOrWhiteSpace(postModel.vchImagenUrl))
            {
                return BadRequest(Response<string>.BadResult("No se proporcionó la URL de la imagen.", string.Empty));
            }

            try
            {
                var client = httpClientFactory.CreateClient();
                client.Timeout = TimeSpan.FromSeconds(25);
                var response = await client.GetAsync(postModel.vchImagenUrl.Trim(), cancellationToken);
                if (!response.IsSuccessStatusCode)
                {
                    return BadRequest(Response<string>.BadResult($"No se pudo descargar la imagen remota (Status {(int)response.StatusCode}).", string.Empty));
                }

                var bytes = await response.Content.ReadAsByteArrayAsync(cancellationToken);
                var contentType = response.Content.Headers.ContentType?.MediaType ?? "image/jpeg";
                var base64 = $"data:{contentType};base64,{Convert.ToBase64String(bytes)}";

                return Ok(new Response<string> { IsSuccess = true, Data = base64, Message = "Imagen descargada con éxito." });
            }
            catch (Exception ex)
            {
                return BadRequest(Response<string>.BadResult($"Error al obtener la imagen: {ex.Message}", string.Empty));
            }
        }

        /// <summary>
        /// Descarga varias imágenes externas por URL en paralelo y las devuelve mapeadas como URL -> Data URL Base64 para html2canvas.
        /// </summary>
        [HttpPost("PostProxyBatchImagesBase64")]
        public async Task<IActionResult> PostProxyBatchImagesBase64([FromBody] PostProxyBatchImages postModel, [FromServices] IHttpClientFactory httpClientFactory, CancellationToken cancellationToken)
        {
            var dict = new Dictionary<string, string>();
            if (postModel?.Urls == null || !postModel.Urls.Any())
            {
                return Ok(new Response<Dictionary<string, string>> { IsSuccess = true, Data = dict, Message = "Ok" });
            }

            var client = httpClientFactory.CreateClient();
            client.Timeout = TimeSpan.FromSeconds(20);

            var tareas = postModel.Urls
                .Distinct()
                .Where(u => !string.IsNullOrWhiteSpace(u) && u.StartsWith("http", StringComparison.OrdinalIgnoreCase))
                .Select(async url =>
                {
                    try
                    {
                        var resp = await client.GetAsync(url.Trim(), cancellationToken);
                        if (resp.IsSuccessStatusCode)
                        {
                            var bytes = await resp.Content.ReadAsByteArrayAsync(cancellationToken);
                            var contentType = resp.Content.Headers.ContentType?.MediaType ?? "image/jpeg";
                            var b64 = $"data:{contentType};base64,{Convert.ToBase64String(bytes)}";
                            return new KeyValuePair<string, string>(url, b64);
                        }
                    }
                    catch
                    {
                        // Fallback silencioso si falla una imagen
                    }
                    return new KeyValuePair<string, string>(url, string.Empty);
                });

            var resultados = await Task.WhenAll(tareas);
            foreach (var kvp in resultados)
            {
                if (!string.IsNullOrWhiteSpace(kvp.Value))
                {
                    dict[kvp.Key] = kvp.Value;
                }
            }

            return Ok(new Response<Dictionary<string, string>> { IsSuccess = true, Data = dict, Message = "Ok" });
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
        /// Noticia que toca publicar (la más reciente), validando que no se haya publicado ya.
        /// </summary>
        [HttpPost("PostGetSiguienteNoticia")]
        public async Task<IActionResult> PostGetSiguienteNoticia(CancellationToken cancellationToken)
        {
            var resultado = await _noticiaAutomaticaService.GetSiguienteAsync(cancellationToken);
            return resultado.IsSuccess ? Ok(resultado) : BadRequest(resultado);
        }

        /// <summary>
        /// Publica la noticia siguiente con el texto y el post (imagen) ya generados en el navegador.
        /// </summary>
        [HttpPost("PostPublicarNoticiaGenerada")]
        public async Task<IActionResult> PostPublicarNoticiaGenerada([FromBody] PostPublicarGenerada postModel, CancellationToken cancellationToken)
        {
            var resultado = postModel == null
                ? Response<string>.BadResult("No se recibieron los datos de la publicación.", string.Empty)
                : await _noticiaAutomaticaService.PublicarGeneradaAsync(postModel.iIdRegistro, postModel.Message, postModel.ImageUrl, cancellationToken);
            var respuesta = new Response<PublicacionAutomatica>
            {
                IsSuccess = resultado.IsSuccess,
                Message = resultado.Message,
                Data = await _publicacionAutomaticaService.GetAsync(cancellationToken)
            };

            return resultado.IsSuccess ? Ok(respuesta) : BadRequest(respuesta);
        }

        /// <summary>
        /// Emprendimiento que toca publicar según la rotación, validando que no se acabe de publicar.
        /// </summary>
        [HttpPost("PostGetSiguienteEmprendimiento")]
        public async Task<IActionResult> PostGetSiguienteEmprendimiento(CancellationToken cancellationToken)
        {
            var resultado = await _emprendimientoAutomaticoService.ObtenerSiguienteAsync(cancellationToken);
            return resultado.IsSuccess ? Ok(resultado) : BadRequest(resultado);
        }

        /// <summary>
        /// Publica el emprendimiento siguiente con el texto y el post (imagen) ya generados en el navegador.
        /// </summary>
        [HttpPost("PostPublicarEmprendimientoGenerado")]
        public async Task<IActionResult> PostPublicarEmprendimientoGenerado([FromBody] PostPublicarGenerada postModel, CancellationToken cancellationToken)
        {
            var resultado = postModel == null
                ? Response<string>.BadResult("No se recibieron los datos de la publicación.", string.Empty)
                : await _emprendimientoAutomaticoService.PublicarGeneradoAsync(postModel.iIdRegistro, postModel.Message, postModel.ImageUrl, cancellationToken);
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
