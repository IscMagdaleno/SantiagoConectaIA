using EngramaCoreStandar.Results;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SantiagoConectaIA.API.EngramaLevels.Domain.Interfaces;
using SantiagoConectaIA.Share.Objects.OpinionModule;
using SantiagoConectaIA.Share.PostModels.OpinionModule;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace SantiagoConectaIA.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OpinionController : ControllerBase
    {
        private readonly IOpinionDomain _opinionDomain;

        public OpinionController(IOpinionDomain opinionDomain)
        {
            _opinionDomain = opinionDomain;
        }

        /// <summary>
        /// Lista opiniones y respuestas de un contenido (noticia, trámite, evento o cápsula).
        /// </summary>
        [HttpPost("PostGetOpiniones")]
        [AllowAnonymous]
        public async Task<IActionResult> PostGetOpiniones([FromBody] PostGetOpiniones postModel)
        {
            var result = await _opinionDomain.GetOpiniones(postModel ?? new PostGetOpiniones());
            if (result.IsSuccess)
            {
                return Ok(result);
            }
            return BadRequest(result);
        }

        /// <summary>
        /// Publica una opinión o respuesta. Requiere ciudadano autenticado.
        /// </summary>
        [HttpPost("PostSaveOpinion")]
        [Authorize(Roles = "Ciudadano")]
        public async Task<IActionResult> PostSaveOpinion([FromBody] PostSaveOpinion postModel)
        {
            var ciudadanoId = GetCiudadanoIdFromToken();
            if (ciudadanoId <= 0)
            {
                return Unauthorized(Response<Opinion>.BadResult("Sesión inválida. Entra de nuevo en Únete.", new Opinion()));
            }

            var result = await _opinionDomain.SaveOpinion(postModel ?? new PostSaveOpinion(), ciudadanoId);
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
