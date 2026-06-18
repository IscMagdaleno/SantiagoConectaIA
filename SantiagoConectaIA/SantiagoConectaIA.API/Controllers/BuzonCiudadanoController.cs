using EngramaCoreStandar.Results;

using Microsoft.AspNetCore.Mvc;

using SantiagoConectaIA.API.EngramaLevels.Domain.Interfaces;
using SantiagoConectaIA.Share.Objects.BuzonCiudadanoModule;
using SantiagoConectaIA.Share.PostModels.BuzonCiudadanoModule;

namespace SantiagoConectaIA.API.Controllers
{
	[ApiController]
	[Route("api/[controller]")]
	public class BuzonCiudadanoController : ControllerBase
	{
		private readonly IBuzonCiudadanoDomain _buzonCiudadanoDomain;
		private readonly ILogger<BuzonCiudadanoController> _logger;

		public BuzonCiudadanoController(
			IBuzonCiudadanoDomain buzonCiudadanoDomain,
			ILogger<BuzonCiudadanoController> logger)
		{
			_buzonCiudadanoDomain = buzonCiudadanoDomain ?? throw new ArgumentNullException(nameof(buzonCiudadanoDomain));
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
		}

		[HttpPost("PostSaveReporte")]
		[ProducesResponseType(typeof(Response<BuzonCiudadano>), 200)]
		public async Task<IActionResult> PostSaveReporte([FromBody] PostSaveBuzonCiudadano model)
		{
			try
			{
				if (model == null)
					return BadRequest(Response<BuzonCiudadano>.BadResult("Payload vacío.", new BuzonCiudadano()));

				if (!ModelState.IsValid)
					return BadRequest(Response<BuzonCiudadano>.BadResult("Modelo inválido.", new BuzonCiudadano()));

				var result = await _buzonCiudadanoDomain.RegistrarReporte(model);
				if (result.IsSuccess)
				{
					return Ok(result);
				}
				return BadRequest(result);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error en PostSaveReporte");
				return BadRequest(Response<BuzonCiudadano>.BadResult("Error interno.", new BuzonCiudadano()));
			}
		}
	}
}
