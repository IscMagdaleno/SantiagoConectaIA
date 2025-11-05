using EngramaCoreStandar.Results;

using Microsoft.AspNetCore.Mvc;

using SantiagoConectaIA.API.EngramaLevels.Domain.Interfaces;
using SantiagoConectaIA.API.Helpers;
using SantiagoConectaIA.Share.Objects.OficinasModule;
using SantiagoConectaIA.Share.PostModels.OficinasModule;


namespace SantiagoConectaIA.API.Controllers
{
	[ApiController]
	[Route("api/[controller]")]
	public class OficinasController : ControllerBase
	{
		private readonly IOficinasDomain _oficinasDomain;
		private readonly IResponseHelper _responseHelper;
		private readonly ILogger<OficinasController> _logger;

		public OficinasController(
			IOficinasDomain oficinasDomain,
			IResponseHelper responseHelper,
			ILogger<OficinasController> logger)
		{
			_oficinasDomain = oficinasDomain ?? throw new ArgumentNullException(nameof(oficinasDomain));
			_responseHelper = responseHelper ?? throw new ArgumentNullException(nameof(responseHelper));
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
		}

		[HttpPost("PostGetOficinas")]
		[ProducesResponseType(typeof(Response<System.Collections.Generic.IEnumerable<Oficina>>), 200)]
		public async Task<IActionResult> PostGetOficinas([FromBody] PostGetOficinas model)
		{
			try
			{
				if (model == null)
					return BadRequest(Response<System.Collections.Generic.IEnumerable<Oficina>>.BadResult("Payload vacío.", null));

				var result = await _oficinasDomain.GetOficinas(model);
				if (result.IsSuccess)
				{
					return Ok(result);
				}
				return BadRequest(result);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error en PostGetOficinas");
				return BadRequest(Response<System.Collections.Generic.IEnumerable<Oficina>>.BadResult("Error interno.", null));
			}
		}

		[HttpPost("PostSearchOficinas")]
		[ProducesResponseType(typeof(Response<PagedList<Oficina>>), 200)]
		public async Task<IActionResult> PostSearchOficinas([FromBody] PostSearchOficinas model)
		{
			try
			{
				if (model == null)
					return BadRequest(Response<PagedList<Oficina>>.BadResult("Payload vacío.", null));

				var result = await _oficinasDomain.SearchOficinas(model);
				if (result.IsSuccess)
				{
					return Ok(result);
				}
				return BadRequest(result);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error en PostSearchOficinas");
				return BadRequest(Response<PagedList<Oficina>>.BadResult("Error interno.", null));
			}
		}

		[HttpPost("PostSaveOficina")]
		[ProducesResponseType(typeof(Response<Oficina>), 200)]
		public async Task<IActionResult> PostSaveOficina([FromBody] PostSaveOficina model)
		{
			try
			{

				var result = await _oficinasDomain.SaveOficina(model);
				if (result.IsSuccess)
				{
					return Ok(result);
				}
				return BadRequest(result);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error en PostSaveOficina");
				return BadRequest(Response<Oficina>.BadResult("Error interno.", new Oficina()));
			}
		}

		[HttpPost("PostGetDependencias")]
		[ProducesResponseType(typeof(Response<System.Collections.Generic.IEnumerable<Dependencia>>), 200)]
		public async Task<IActionResult> PostGetDependencias([FromBody] PostGetDependencias model)
		{
			try
			{
				if (model == null)
					return BadRequest(Response<System.Collections.Generic.IEnumerable<Dependencia>>.BadResult("Payload vacío.", null));

				var result = await _oficinasDomain.GetDependencias(model);
				if (result.IsSuccess)
				{
					return Ok(result);
				}
				return BadRequest(result);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error en PostGetDependencias");
				return BadRequest(Response<System.Collections.Generic.IEnumerable<Dependencia>>.BadResult("Error interno.", null));
			}
		}

		[HttpPost("PostSaveDependencia")]
		[ProducesResponseType(typeof(Response<Dependencia>), 200)]
		public async Task<IActionResult> PostSaveDependencia([FromBody] PostSaveDependencia model)
		{
			try
			{

				var result = await _oficinasDomain.SaveDependencia(model);
				if (result.IsSuccess)
				{
					return Ok(result);
				}
				return BadRequest(result);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error en PostSaveDependencia");
				return BadRequest(Response<Dependencia>.BadResult("Error interno.", new Dependencia()));
			}
		}

		[HttpPost("PostLinkOficinaTramite")]
		[ProducesResponseType(typeof(Response<int>), 200)]
		public async Task<IActionResult> PostLinkOficinaTramite([FromBody] PostLinkOficinaTramite model)
		{
			try
			{
				if (model == null)
					return BadRequest(Response<int>.BadResult("Payload vacío.", 0));

				if (!ModelState.IsValid)
					return BadRequest(Response<int>.BadResult("Modelo inválido.", 0));

				var result = await _oficinasDomain.LinkOficinaTramite(model);
				if (result.IsSuccess)
				{
					return Ok(result);
				}
				return BadRequest(result);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error en PostLinkOficinaTramite");
				return BadRequest(Response<int>.BadResult("Error interno.", 0));
			}
		}

		[HttpPost("PostGetOficinasPorTramite")]
		[ProducesResponseType(typeof(Response<System.Collections.Generic.IEnumerable<OficinaPorTramite>>), 200)]
		public async Task<IActionResult> PostGetOficinasPorTramite([FromBody] PostGetOficinasPorTramite model)
		{
			try
			{
				if (model == null)
					return BadRequest(Response<System.Collections.Generic.IEnumerable<OficinaPorTramite>>.BadResult("Payload vacío.", null));

				var result = await _oficinasDomain.GetOficinasPorTramite(model);
				if (result.IsSuccess)
				{
					return Ok(result);
				}
				return BadRequest(result);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error en PostGetOficinasPorTramite");
				return BadRequest(Response<System.Collections.Generic.IEnumerable<OficinaPorTramite>>.BadResult("Error interno.", null));
			}
		}
	}
}


