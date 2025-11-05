using Microsoft.AspNetCore.Mvc;

using SantiagoConectaIA.API.EngramaLevels.Domain.Interfaces;
using SantiagoConectaIA.Share.PostModels.TramitesModule;

namespace SantiagoConectaIA.API.Controllers
{
	[ApiController]
	[Route("api/[controller]")]
	public class TramitesController : ControllerBase
	{
		private readonly ITramiteDominio tramiteDominio;

		public TramitesController(ITramiteDominio tramiteDominio)
		{
			this.tramiteDominio = tramiteDominio;
		}


		/// <summary>
		/// Consultas los tramites guardados en base de datos
		/// </summary>
		/// <param name="postModel"></param>
		/// <returns></returns>
		[HttpPost("PostGetTramites")]
		public async Task<IActionResult> PostGetTramites([FromBody] PostGetTramites postModel)
		{
			var result = await tramiteDominio.GetTramites(postModel);
			if (result.IsSuccess)
			{
				return Ok(result);
			}
			return BadRequest(result);
		}


		/// <summary>
		/// Consulta tramites específicos
		/// </summary>
		/// <param name="postModel"></param>
		/// <returns></returns>
		[HttpPost("PostSearchTramites")]
		public async Task<IActionResult> PostSearchTramites([FromBody] PostSearchTramites postModel)
		{
			var result = await tramiteDominio.SearchTramites(postModel);
			if (result.IsSuccess)
			{
				return Ok(result);
			}
			return BadRequest(result);
		}


		[HttpPost("PostSaveTramite")]
		public async Task<IActionResult> PostSaveTramite([FromBody] PostSaveTramite postModel)
		{
			var result = await tramiteDominio.SaveTramite(postModel);
			if (result.IsSuccess)
			{
				return Ok(result);
			}
			return BadRequest(result);
		}


		[HttpPost("PostGetRequisitosPorTramite")]
		public async Task<IActionResult> PostGetRequisitosPorTramite([FromBody] PostGetRequisitosPorTramite postModel)
		{
			var result = await tramiteDominio.GetRequisitosPorTramite(postModel);
			if (result.IsSuccess)
			{
				return Ok(result);
			}
			return BadRequest(result);
		}


		[HttpPost("PostSaveRequisito")]
		public async Task<IActionResult> PostSaveRequisito([FromBody] PostSaveRequisito postModel)
		{
			var result = await tramiteDominio.SaveRequisito(postModel);
			if (result.IsSuccess)
			{
				return Ok(result);
			}
			return BadRequest(result);
		}

	}
}
