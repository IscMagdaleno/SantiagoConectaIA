using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using SantiagoConectaIA.API.EngramaLevels.Domain.Interfaces;
using SantiagoConectaIA.Share.PostModels.PageVisitsModule;

namespace SantiagoConectaIA.API.Controllers
{
	[ApiController]
	[Route("api/[controller]")]
	public class PageVisitsController : ControllerBase
	{
		private readonly IPageVisitsDomain _pageVisitsDomain;

		public PageVisitsController(IPageVisitsDomain pageVisitsDomain)
		{
			_pageVisitsDomain = pageVisitsDomain;
		}

		/// <summary>
		/// Consulta las estadísticas de visitas a páginas (Dashboard) utilizando la metodología Engrama.
		/// </summary>
		/// <param name="postModel">Modelo de entrada con los parámetros para la consulta.</param>
		/// <returns>Respuesta con los datos consultados o un mensaje de error.</returns>
		[HttpPost("PostGetPageVisitsStats")]
		public async Task<IActionResult> PostGetPageVisitsStats([FromBody] PostGetPageVisitsStats postModel)
		{
			var result = await _pageVisitsDomain.GetPageVisitsStats(postModel);
			if (result.IsSuccess)
			{
				return Ok(result);
			}
			return BadRequest(result);
		}
	}
}
