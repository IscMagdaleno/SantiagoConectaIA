using EngramaCoreStandar.Results;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using SantiagoConectaIA.API.EngramaLevels.Domain.Interfaces;
using SantiagoConectaIA.API.Services;
using SantiagoConectaIA.Share.Objects.MemesModule;
using SantiagoConectaIA.Share.PostModels.MemesModule;

using System.Security.Claims;

namespace SantiagoConectaIA.API.Controllers
{
	/// <summary>Memes con IA: plantillas (tendencias y biblioteca), creación, uso diario e historial. Solo administración.</summary>
	[ApiController]
	[Authorize(Policy = AuthPolicies.Administracion)]
	[Route("api/[controller]")]
	public class MemesController : ApiControllerBase
	{
		private readonly IMemeService _memeService;
		private readonly IMemesDomain _memesDomain;

		public MemesController(IMemeService memeService, IMemesDomain memesDomain)
		{
			_memeService = memeService;
			_memesDomain = memesDomain;
		}

		/// <summary>Plantillas de meme: tendencias (24h, 7d, 30d) o biblioteca con búsqueda y categoría.</summary>
		[HttpPost("PostGetPlantillasMeme")]
		public async Task<IActionResult> PostGetPlantillasMeme([FromBody] PostGetPlantillasMeme postModel, CancellationToken cancellationToken)
		{
			return Resultado(await _memeService.GetPlantillasAsync(postModel, cancellationToken));
		}

		/// <summary>Crea un meme con Gemini a partir de la plantilla elegida y la idea; guarda la imagen en Azure Blob.</summary>
		[HttpPost("PostCrearMeme")]
		public async Task<IActionResult> PostCrearMeme([FromBody] PostCrearMeme postModel, CancellationToken cancellationToken)
		{
			return Resultado(await _memeService.CrearAsync(ObtenerIdUsuario(), postModel, cancellationToken));
		}

		/// <summary>Memes creados hoy y límite diario.</summary>
		[HttpPost("PostGetResumen")]
		public async Task<IActionResult> PostGetResumen(CancellationToken cancellationToken)
		{
			return Resultado(await _memeService.GetResumenAsync(cancellationToken));
		}

		/// <summary>Memes recientes (compartidos entre administradores), sin los quitados.</summary>
		[HttpPost("PostGetMemesRecientes")]
		public async Task<IActionResult> PostGetMemesRecientes([FromBody] PostGetMemesRecientes postModel)
		{
			var top = Math.Clamp(postModel?.iTop > 0 ? postModel.iTop : 12, 1, 50);
			return Resultado(await _memesDomain.GetRecientes(top));
		}

		/// <summary>Quita un meme del historial.</summary>
		[HttpPost("PostDescartarMeme")]
		public async Task<IActionResult> PostDescartarMeme([FromBody] PostDescartarMeme postModel)
		{
			return Resultado(await _memesDomain.Descartar(postModel?.iIdMeme ?? 0));
		}

		private int ObtenerIdUsuario() =>
			int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : 0;
	}
}
