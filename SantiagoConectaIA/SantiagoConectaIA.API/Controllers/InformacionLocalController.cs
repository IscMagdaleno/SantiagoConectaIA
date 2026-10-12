using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using SantiagoConectaIA.API.EngramaLevels.Domain.Interfaces;
using SantiagoConectaIA.API.Services;
using SantiagoConectaIA.Share.PostModels.InformacionLocalModule;

namespace SantiagoConectaIA.API.Controllers
{
	/// <summary>
	/// Controlador para la tabla InformacionLocal (Datos Curiosos)
	/// </summary>
	[ApiController]
	[Route("api/[controller]")]
	public class InformacionLocalController : ControllerBase
	{
		private readonly IInformacionLocalDomain _informacionLocalDomain;

		public InformacionLocalController(IInformacionLocalDomain informacionLocalDomain)
		{
			_informacionLocalDomain = informacionLocalDomain;
		}

		[HttpPost("PostGetAllInformacionLocal")]
		public async Task<IActionResult> PostGetAllInformacionLocal()
		{
			var result = await _informacionLocalDomain.GetInformacionLocal();
			if (result.IsSuccess)
				return Ok(result);
			return BadRequest(result);
		}

		[HttpPost("PostGetInformacionLocalByID")]
		public async Task<IActionResult> PostGetInformacionLocalByID([FromBody] PostGetInformacionLocalByID postModel)
		{
			var result = await _informacionLocalDomain.GetInformacionLocalById(postModel.iIdInformacionLocal);
			if (result.IsSuccess)
				return Ok(result);
			return BadRequest(result);
		}

		[HttpPost("PostGetformacionLocalByText")]
		public async Task<IActionResult> PostGetformacionLocalByText([FromBody] PostGetInformacionLocal postModel)
		{
			var result = await _informacionLocalDomain.PostGetformacionLocalByText(postModel);
			if (result.IsSuccess)
				return Ok(result);
			return BadRequest(result);
		}

		[HttpPost("PostSaveInformacionLocal")]
		[Authorize(Policy = AuthPolicies.Administracion)]
		public async Task<IActionResult> PostSaveInformacionLocal([FromBody] PostSaveInformacionLocal postModel)
		{
			var result = await _informacionLocalDomain.SaveInformacionLocal(postModel);
			if (result.IsSuccess)
				return Ok(result);
			return BadRequest(result);
		}

		/// <summary>
		/// Consulta una lista de registros de InformacionLocal
		/// </summary>
		/// <param name="postModel">Modelo con parámetros de búsqueda</param>
		/// <returns>Registros o mensaje de error</returns>
		[HttpPost("PostGetInformacionLocal")]
		public async Task<IActionResult> PostGetInformacionLocal([FromBody] PostGetInformacionLocal postModel)
		{
			var result = await _informacionLocalDomain.GetInformacionLocal(postModel);
			if (result.IsSuccess)
			{
				return Ok(result);
			}
			return BadRequest(result);
		}
	}
}
