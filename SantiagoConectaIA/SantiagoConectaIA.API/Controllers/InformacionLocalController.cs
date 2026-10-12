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
	public class InformacionLocalController : ApiControllerBase
	{
		private readonly IInformacionLocalDomain _informacionLocalDomain;

		public InformacionLocalController(IInformacionLocalDomain informacionLocalDomain)
		{
			_informacionLocalDomain = informacionLocalDomain;
		}

		[HttpPost("PostGetInformacionLocalByID")]
		public async Task<IActionResult> PostGetInformacionLocalByID([FromBody] PostGetInformacionLocalByID postModel)
		{
			var result = await _informacionLocalDomain.GetInformacionLocalById(postModel.iIdInformacionLocal);
			return Resultado(result);
		}

		[HttpPost("PostSaveInformacionLocal")]
		[Authorize(Policy = AuthPolicies.Administracion)]
		public async Task<IActionResult> PostSaveInformacionLocal([FromBody] PostSaveInformacionLocal postModel)
		{
			var result = await _informacionLocalDomain.SaveInformacionLocal(postModel);
			return Resultado(result);
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
			return Resultado(result);
		}
	}
}
