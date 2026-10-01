using Microsoft.AspNetCore.Mvc;
using EngramaCoreStandar.Results;
using SantiagoConectaIA.API.Services;
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

        public PublicacionesController(
            IFacebookPublishService facebookPublishService,
            IGeminiPublicacionService geminiPublicacionService,
            IGeminiImagenService geminiImagenService,
            IPublicacionAutomaticaService publicacionAutomaticaService,
            IEmprendimientoAutomaticoService emprendimientoAutomaticoService)
        {
            _facebookPublishService = facebookPublishService;
            _geminiPublicacionService = geminiPublicacionService;
            _geminiImagenService = geminiImagenService;
            _publicacionAutomaticaService = publicacionAutomaticaService;
            _emprendimientoAutomaticoService = emprendimientoAutomaticoService;
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
