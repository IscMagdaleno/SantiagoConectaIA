using EngramaCoreStandar.Results;
using Microsoft.AspNetCore.Mvc;
using SantiagoConectaIA.API.EngramaLevels.Domain.Interfaces;
using SantiagoConectaIA.Share.Objects.FeedModule;
using SantiagoConectaIA.Share.PostModels.FeedModule;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SantiagoConectaIA.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FeedController : ControllerBase
    {
        private readonly IFeedDomain _feedDomain;

        public FeedController(IFeedDomain feedDomain)
        {
            _feedDomain = feedDomain;
        }

        /// <summary>
        /// Obtiene una página del feed mixto de contenido público.
        /// </summary>
        [HttpPost("PostGetFeed")]
        public async Task<IActionResult> PostGetFeed([FromBody] PostGetFeed postModel)
        {
            var result = await _feedDomain.GetFeed(postModel ?? new PostGetFeed());
            if (result.IsSuccess)
            {
                return Ok(result);
            }
            return BadRequest(result);
        }

        /// <summary>
        /// Busca contenido en trámites, noticias, eventos y cápsulas.
        /// </summary>
        [HttpPost("PostSearchFeed")]
        public async Task<IActionResult> PostSearchFeed([FromBody] PostSearchFeed postModel)
        {
            var result = await _feedDomain.SearchFeed(postModel ?? new PostSearchFeed());
            if (result.IsSuccess)
            {
                return Ok(result);
            }
            return BadRequest(result);
        }
    }
}
