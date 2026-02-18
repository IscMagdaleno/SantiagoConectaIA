using Microsoft.AspNetCore.Mvc;
using SantiagoConectaIA.API.EngramaLevels.Domain.Interfaces;
using SantiagoConectaIA.Share.Objects.NoticiasModule;
using SantiagoConectaIA.Share.PostModels.NoticiasModule;
using EngramaCoreStandar.Results;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SantiagoConectaIA.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class NoticiasController : ControllerBase
    {
        private readonly INoticiasDomain _noticiasDomain;

        public NoticiasController(INoticiasDomain noticiasDomain)
        {
            _noticiasDomain = noticiasDomain;
        }

        /// <summary>
        /// Obtiene el listado de noticias.
        /// </summary>
        [HttpPost("PostGetNoticias")]
        public async Task<IActionResult> PostGetNoticias([FromBody] PostGetNoticias postModel)
        {
            var result = await _noticiasDomain.GetNoticias(postModel);
            if (result.IsSuccess)
            {
                return Ok(result);
            }
            return BadRequest(result);
        }

        /// <summary>
        /// Guarda o actualiza una noticia.
        /// </summary>
        [HttpPost("PostSaveNoticia")]
        public async Task<IActionResult> PostSaveNoticia([FromBody] PostSaveNoticia postModel)
        {
            var result = await _noticiasDomain.SaveNoticia(postModel);
            if (result.IsSuccess)
            {
                return Ok(result);
            }
            return BadRequest(result);
        }
    }
}
