using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using SantiagoConectaIA.API.EngramaLevels.Domain.Interfaces;
using SantiagoConectaIA.API.Services;
using SantiagoConectaIA.Share.PostModels.NoticiasModule;

namespace SantiagoConectaIA.API.Controllers
{
	[ApiController]
	[Route("api/[controller]")]
	public class NoticiasController : ApiControllerBase
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
			return Resultado(result);
		}

		/// <summary>
		/// Guarda o actualiza una noticia.
		/// </summary>
		[HttpPost("PostSaveNoticia")]
		[Authorize(Policy = AuthPolicies.Administracion)]
		public async Task<IActionResult> PostSaveNoticia([FromBody] PostSaveNoticia postModel)
		{
			var result = await _noticiasDomain.SaveNoticia(postModel);
			return Resultado(result);
		}
	}
}
