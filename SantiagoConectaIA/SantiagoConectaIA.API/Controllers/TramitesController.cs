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
		/// <returns>Resultado de la operación con la lista de trámites.</returns>
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
		/// <returns>Resultado de la operación con la lista de trámites que coinciden con la búsqueda.</returns>
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


		/// <summary>
		/// Guarda o actualiza un trámite en la base de datos.
		/// </summary>
		/// <param name="postModel">Modelo con la información del trámite a guardar.</param>
		/// <returns>Resultado de la operación de guardado.</returns>
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


		/// <summary>
		/// Obtiene los requisitos asociados a un trámite específico.
		/// </summary>
		/// <param name="postModel">Modelo con el identificador del trámite.</param>
		/// <returns>Resultado de la operación con la lista de requisitos.</returns>
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


		/// <summary>
		/// Guarda o actualiza un requisito asociado a un trámite.
		/// </summary>
		/// <param name="postModel">Modelo con la información del requisito a guardar.</param>
		/// <returns>Resultado de la operación de guardado.</returns>
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


		/// <summary>
		/// Obtiene una lista de trámites en formato de tarjeta (resumen).
		/// </summary>
		/// <param name="postModel">Modelo de consulta para obtener los trámites.</param>
		/// <returns>Resultado de la operación con la lista de trámites en formato de tarjeta.</returns>
		[HttpPost("PostGetTramitesCard")]
		public async Task<IActionResult> PostGetTramitesCard([FromBody] PostGetTramites postModel)
		{
			var result = await tramiteDominio.GetTramitesCard(postModel);
			if (result.IsSuccess)
			{
				return Ok(result);
			}
			return BadRequest(result);
		}

		/// <summary>
		/// Obtiene el detalle completo de un trámite, incluyendo requisitos, pasos y documentos.
		/// </summary>
		/// <param name="postModel">Modelo con el identificador del trámite para obtener el detalle.</param>
		/// <returns>Resultado de la operación con la información detallada del trámite.</returns>
		[HttpPost("PostGetTramiteDetalle")]
		public async Task<IActionResult> PostGetTramiteDetalle([FromBody] PostGetTramiteDetalle postModel)
		{
			var result = await tramiteDominio.GetTramiteDetalle(postModel);
			if (result.IsSuccess)
			{
				return Ok(result);
			}
			return BadRequest(result);
		}

		/// <summary>
		/// Guarda los documentos relacionados a un Tramite
		/// </summary>
		/// <param name="postModel">Modelo con la información del documento a guardar.</param>
		/// <returns>Resultado de la operación de guardado del documento.</returns>
		[HttpPost("PostSaveDocumento")]
		public async Task<IActionResult> PostSaveDocumento([FromBody] PostSaveDocumento postModel)
		{
			var result = await tramiteDominio.SaveDocumento(postModel);
			if (result.IsSuccess)
			{
				return Ok(result);
			}
			return BadRequest(result);
		}

	}
}