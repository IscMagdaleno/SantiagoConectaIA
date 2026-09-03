using EngramaCoreStandar.Results;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SantiagoConectaIA.API.EngramaLevels.Domain.Interfaces;
using SantiagoConectaIA.Share.Objects.PublicacionCiudadanoModule;
using SantiagoConectaIA.Share.PostModels.PublicacionCiudadanoModule;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace SantiagoConectaIA.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PublicacionesCiudadanoController : ControllerBase
    {
        private readonly IPublicacionesCiudadanoDomain _publicacionesDomain;

        public PublicacionesCiudadanoController(IPublicacionesCiudadanoDomain publicacionesDomain)
        {
            _publicacionesDomain = publicacionesDomain;
        }

        /// <summary>
        /// Obtiene publicaciones ciudadanas públicas para el feed de la comunidad con paginación.
        /// </summary>
        [HttpPost("PostGetPublicacionesCiudadano")]
        [AllowAnonymous]
        public async Task<IActionResult> PostGetPublicacionesCiudadano([FromBody] PostGetPublicacionesCiudadano postModel)
        {
            var result = await _publicacionesDomain.GetPublicacionesCiudadano(postModel ?? new PostGetPublicacionesCiudadano());
            if (result.IsSuccess)
            {
                return Ok(result);
            }
            return BadRequest(result);
        }

        /// <summary>
        /// Guarda una nueva publicación o actualiza una existente con fotos. Requiere ciudadano autenticado.
        /// </summary>
        [HttpPost("PostSavePublicacionCiudadano")]
        [Authorize(Roles = "Ciudadano")]
        public async Task<IActionResult> PostSavePublicacionCiudadano([FromBody] PostSavePublicacionCiudadano postModel)
        {
            var ciudadanoId = GetCiudadanoIdFromToken();
            if (ciudadanoId <= 0)
            {
                return Unauthorized(Response<PublicacionCiudadano>.BadResult("Sesión inválida. Inicia sesión en Únete.", new PublicacionCiudadano()));
            }

            var result = await _publicacionesDomain.SavePublicacionCiudadano(postModel ?? new PostSavePublicacionCiudadano(), ciudadanoId);
            if (result.IsSuccess)
            {
                return Ok(result);
            }
            return BadRequest(result);
        }

        /// <summary>
        /// Obtiene las publicaciones propias del ciudadano logueado para su panel en /unete.
        /// </summary>
        [HttpPost("PostGetMisPublicacionesCiudadano")]
        [Authorize(Roles = "Ciudadano")]
        public async Task<IActionResult> PostGetMisPublicacionesCiudadano([FromBody] PostGetMisPublicacionesCiudadano postModel)
        {
            var ciudadanoId = GetCiudadanoIdFromToken();
            if (ciudadanoId <= 0)
            {
                return Unauthorized(Response<IEnumerable<PublicacionCiudadano>>.BadResult("Sesión inválida.", new List<PublicacionCiudadano>()));
            }

            var result = await _publicacionesDomain.GetMisPublicacionesCiudadano(postModel ?? new PostGetMisPublicacionesCiudadano(), ciudadanoId);
            if (result.IsSuccess)
            {
                return Ok(result);
            }
            return BadRequest(result);
        }

        /// <summary>
        /// Da de baja lógica una publicación del propio ciudadano.
        /// </summary>
        [HttpPost("PostDeletePublicacionCiudadano")]
        [Authorize(Roles = "Ciudadano")]
        public async Task<IActionResult> PostDeletePublicacionCiudadano([FromBody] PostDeletePublicacionCiudadano postModel)
        {
            var ciudadanoId = GetCiudadanoIdFromToken();
            if (ciudadanoId <= 0)
            {
                return Unauthorized(Response<string>.BadResult("Sesión inválida.", string.Empty));
            }

            var result = await _publicacionesDomain.DeletePublicacionCiudadano(postModel ?? new PostDeletePublicacionCiudadano(), ciudadanoId);
            if (result.IsSuccess)
            {
                return Ok(result);
            }
            return BadRequest(result);
        }

        private int GetCiudadanoIdFromToken()
        {
            var claim = User.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? User.FindFirstValue("sub");
            return int.TryParse(claim, out var id) ? id : 0;
        }
    }
}
