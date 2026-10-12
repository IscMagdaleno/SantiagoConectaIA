using Microsoft.AspNetCore.Mvc;

using SantiagoConectaIA.API.EngramaLevels.Domain.Interfaces;
using SantiagoConectaIA.Share.PostModels.FeedModule;

namespace SantiagoConectaIA.API.Controllers
{
	[ApiController]
	[Route("api/[controller]")]
	public class FeedController : ApiControllerBase
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
			return Resultado(result);
		}

		/// <summary>
		/// Busca contenido en trámites, noticias, eventos y cápsulas.
		/// </summary>
		[HttpPost("PostSearchFeed")]
		public async Task<IActionResult> PostSearchFeed([FromBody] PostSearchFeed postModel)
		{
			var result = await _feedDomain.SearchFeed(postModel ?? new PostSearchFeed());
			return Resultado(result);
		}
	}
}
