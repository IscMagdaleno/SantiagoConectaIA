using Microsoft.AspNetCore.Mvc;

using SantiagoConectaIA.API.EngramaLevels.Domain.Interfaces;
using SantiagoConectaIA.Share.PostModels.EmprendimientosFeedModule;

namespace SantiagoConectaIA.API.Controllers
{
	[ApiController]
	[Route("api/[controller]")]
	public class EmprendimientosFeedController : ApiControllerBase
	{
		private readonly IEmprendimientosFeedDomain _domain;

		public EmprendimientosFeedController(IEmprendimientosFeedDomain domain)
		{
			_domain = domain;
		}

		[HttpPost("PostGetEmprendimientosFeed")]
		public async Task<IActionResult> PostGetEmprendimientosFeed([FromBody] PostGetEmprendimientosFeed postModel)
		{
			var result = await _domain.GetFeed(postModel ?? new PostGetEmprendimientosFeed());
			return Resultado(result);
		}

		[HttpPost("PostSearchEmprendimientosFeed")]
		public async Task<IActionResult> PostSearchEmprendimientosFeed([FromBody] PostSearchEmprendimientosFeed postModel)
		{
			var result = await _domain.SearchFeed(postModel ?? new PostSearchEmprendimientosFeed());
			return Resultado(result);
		}
	}
}
