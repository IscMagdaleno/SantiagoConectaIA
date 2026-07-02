using Microsoft.AspNetCore.Mvc;
using SantiagoConectaIA.API.EngramaLevels.Domain.Interfaces.EventosModule;
using SantiagoConectaIA.Share.PostClass.EventosModulo;
using System.Threading.Tasks;

namespace SantiagoConectaIA.API.Controllers
{
    /// <summary>
    /// Controlador para el modulo de Eventos, siguiendo la metodologia Engrama.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class EventosController : ControllerBase
    {
        private readonly IEventosDomain _eventosDomain;

        /// <summary>
        /// Inicializa el controlador con la dependencia de la capa de Dominio.
        /// </summary>
        public EventosController(IEventosDomain eventosDomain)
        {
            _eventosDomain = eventosDomain;
        }

        /// <summary>
        /// Consulta la lista de eventos filtrados por ID, categoría, estatus o destacado.
        /// </summary>
        [HttpPost("PostGetEventos")]
        public async Task<IActionResult> PostGetEventos([FromBody] PostGetEventos postModel)
        {
            var result = await _eventosDomain.GetEventos(postModel);
            if (result.IsSuccess)
            {
                return Ok(result);
            }
            return BadRequest(result);
        }

        /// <summary>
        /// Guarda o actualiza un registro en la tabla Eventos.
        /// </summary>
        [HttpPost("PostSaveEvento")]
        public async Task<IActionResult> PostSaveEvento([FromBody] PostSaveEvento postModel)
        {
            var result = await _eventosDomain.SaveEvento(postModel);
            if (result.IsSuccess) return Ok(result);
            return BadRequest(result);
        }

        /// <summary>
        /// Consulta el catálogo de categorías de eventos.
        /// </summary>
        [HttpPost("PostGetCategoriaEventos")]
        public async Task<IActionResult> PostGetCategoriaEventos([FromBody] PostGetCategoriaEventos postModel)
        {
            var result = await _eventosDomain.GetCategoriaEventos(postModel);
            if (result.IsSuccess) return Ok(result);
            return BadRequest(result);
        }

        /// <summary>
        /// Guarda o actualiza una categoría de evento.
        /// </summary>
        [HttpPost("PostSaveCategoriaEvento")]
        public async Task<IActionResult> PostSaveCategoriaEvento([FromBody] PostSaveCategoriaEvento postModel)
        {
            var result = await _eventosDomain.SaveCategoriaEvento(postModel);
            if (result.IsSuccess) return Ok(result);
            return BadRequest(result);
        }

        /// <summary>
        /// Consulta las imágenes asociadas a un registro (genérico).
        /// </summary>
        [HttpPost("PostGetImagenesRegistro")]
        public async Task<IActionResult> PostGetImagenesRegistro([FromBody] PostGetImagenesRegistro postModel)
        {
            var result = await _eventosDomain.GetImagenesRegistro(postModel);
            if (result.IsSuccess) return Ok(result);
            return BadRequest(result);
        }

        /// <summary>
        /// Guarda una imagen para un registro (genérico).
        /// </summary>
        [HttpPost("PostSaveImagenRegistro")]
        public async Task<IActionResult> PostSaveImagenRegistro([FromBody] PostSaveImagenRegistro postModel)
        {
            var result = await _eventosDomain.SaveImagenRegistro(postModel);
            if (result.IsSuccess) return Ok(result);
            return BadRequest(result);
        }

        /// <summary>
        /// Elimina (desactiva) una imagen de registro (genérico).
        /// </summary>
        [HttpPost("PostDeleteImagenRegistro")]
        public async Task<IActionResult> PostDeleteImagenRegistro([FromBody] PostDeleteImagenRegistro postModel)
        {
            var result = await _eventosDomain.DeleteImagenRegistro(postModel);
            if (result.IsSuccess) return Ok(result);
            return BadRequest(result);
        }

        /// <summary>
        /// Consulta el detalle completo de un evento incluyendo todas sus imágenes relacionadas.
        /// </summary>
        [HttpPost("PostGetEventoDetalle")]
        public async Task<IActionResult> PostGetEventoDetalle([FromBody] PostGetEventoDetalle postModel)
        {
            var result = await _eventosDomain.GetEventoDetalle(postModel);
            if (result.IsSuccess) return Ok(result);
            return BadRequest(result);
        }

        /// <summary>
        /// Consulta las sucursales/locales asociadas a un evento.
        /// </summary>
        [HttpPost("PostGetEventosSucursales")]
        public async Task<IActionResult> PostGetEventosSucursales([FromBody] PostGetEventosSucursales postModel)
        {
            var result = await _eventosDomain.GetEventosSucursales(postModel);
            if (result.IsSuccess) return Ok(result);
            return BadRequest(result);
        }

        /// <summary>
        /// Guarda o actualiza una sucursal/local de evento.
        /// </summary>
        [HttpPost("PostSaveSucursalEvento")]
        public async Task<IActionResult> PostSaveSucursalEvento([FromBody] PostSaveSucursalEvento postModel)
        {
            var result = await _eventosDomain.SaveSucursalEvento(postModel);
            if (result.IsSuccess) return Ok(result);
            return BadRequest(result);
        }
    }
}
