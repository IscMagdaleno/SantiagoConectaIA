using EngramaCoreStandar.Results;
using Microsoft.AspNetCore.Mvc;

using SantiagoConectaIA.API.EngramaLevels.Domain.Interfaces;
using SantiagoConectaIA.Share.Objects.AnalyticsModule;
using SantiagoConectaIA.Share.PostModels.AnalyticsModule;

namespace SantiagoConectaIA.API.Controllers
{
	[ApiController]
	[Route("api/[controller]")]
	public class AnalyticsController : ControllerBase
	{
		private readonly IAnalyticsDomain _analyticsDomain;
		private readonly ILogger<AnalyticsController> _logger;

		/// <summary>
		/// Inyección de dependencias del dominio de analytics y logger.
		/// </summary>
		public AnalyticsController(
			IAnalyticsDomain analyticsDomain,
			ILogger<AnalyticsController> logger)
		{
			_analyticsDomain = analyticsDomain ?? throw new ArgumentNullException(nameof(analyticsDomain));
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
		}

		/// <summary>
		/// Registra una visita a una página del sitio web.
		/// </summary>
		/// <param name="model">Datos de la visita a registrar.</param>
		/// <returns>Resultado con el ID de la visita y si es única.</returns>
		[HttpPost("PostSavePageVisit")]
		[ProducesResponseType(typeof(Response<PageVisitSaveResult>), 200)]
		public async Task<IActionResult> PostSavePageVisit([FromBody] PostSavePageVisit model)
		{
			try
			{
				if (model == null)
					return BadRequest(Response<PageVisitSaveResult>.BadResult("Payload vacío.", new PageVisitSaveResult()));

				if (!ModelState.IsValid)
					return BadRequest(Response<PageVisitSaveResult>.BadResult("Modelo inválido.", new PageVisitSaveResult()));

				var result = await _analyticsDomain.SavePageVisit(model);
				if (result.IsSuccess)
				{
					return Ok(result);
				}
				return BadRequest(result);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error en PostSavePageVisit");
				return BadRequest(Response<PageVisitSaveResult>.BadResult("Error interno.", new PageVisitSaveResult()));
			}
		}

		/// <summary>
		/// Obtiene el resumen de visitas (totales, únicas, nuevas, recurrentes).
		/// </summary>
		/// <param name="model">Rango de fechas opcional.</param>
		/// <returns>Estadísticas generales de visitas.</returns>
		[HttpPost("PostGetPageVisitsSummary")]
		[ProducesResponseType(typeof(IEnumerable<PageVisitSummary>), 200)]
		public async Task<IActionResult> PostGetPageVisitsSummary([FromBody] AnalyticsDateRange model)
		{
			try
			{
				var result = await _analyticsDomain.GetPageVisitsSummary(model?.dtStartDate, model?.dtEndDate);
				return Ok(result);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error en PostGetPageVisitsSummary");
				return BadRequest(new List<PageVisitSummary> { new() { bResult = false, vchMessage = ex.Message } });
			}
		}

		/// <summary>
		/// Obtiene las visitas agrupadas por página.
		/// </summary>
		/// <param name="model">Rango de fechas opcional.</param>
		/// <returns>Listado de visitas por página con conteo.</returns>
		[HttpPost("PostGetPageVisitsByPage")]
		[ProducesResponseType(typeof(IEnumerable<PageVisitByPage>), 200)]
		public async Task<IActionResult> PostGetPageVisitsByPage([FromBody] AnalyticsDateRange model)
		{
			try
			{
				var result = await _analyticsDomain.GetPageVisitsByPage(model?.dtStartDate, model?.dtEndDate);
				return Ok(result);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error en PostGetPageVisitsByPage");
				return BadRequest(new List<PageVisitByPage> { new() { bResult = false, vchMessage = ex.Message } });
			}
		}

		/// <summary>
		/// Obtiene el tráfico diario del sitio web.
		/// </summary>
		/// <param name="model">Rango de fechas opcional.</param>
		/// <returns>Datos de tráfico diario para gráficas.</returns>
		[HttpPost("PostGetDailyTraffic")]
		[ProducesResponseType(typeof(IEnumerable<DailyTraffic>), 200)]
		public async Task<IActionResult> PostGetDailyTraffic([FromBody] AnalyticsDateRange model)
		{
			try
			{
				var result = await _analyticsDomain.GetDailyTraffic(model?.dtStartDate, model?.dtEndDate);
				return Ok(result);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error en PostGetDailyTraffic");
				return BadRequest(new List<DailyTraffic> { new() { bResult = false, vchMessage = ex.Message } });
			}
		}

		/// <summary>
		/// Obtiene las visitas más recientes al sitio web.
		/// </summary>
		/// <param name="model">Número de registros a obtener.</param>
		/// <returns>Listado de visitas recientes.</returns>
		[HttpPost("PostGetRecentVisits")]
		[ProducesResponseType(typeof(IEnumerable<PageVisit>), 200)]
		public async Task<IActionResult> PostGetRecentVisits([FromBody] AnalyticsTopRows model)
		{
			try
			{
				var topRows = model?.iTopRows ?? 100;
				var result = await _analyticsDomain.GetRecentVisits(topRows);
				return Ok(result);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error en PostGetRecentVisits");
				return BadRequest(new List<PageVisit> { new() { vchPageUrl = "Error", vchPageName = ex.Message } });
			}
		}
	}

	/// <summary>
	/// Modelo para rango de fechas en consultas de analytics.
	/// </summary>
	public class AnalyticsDateRange
	{
		/// <summary>Fecha de inicio del rango.</summary>
		public DateTime? dtStartDate { get; set; }
		/// <summary>Fecha de fin del rango.</summary>
		public DateTime? dtEndDate { get; set; }
	}

	/// <summary>
	/// Modelo para obtener N registros recientes.
	/// </summary>
	public class AnalyticsTopRows
	{
		/// <summary>Número de registros a obtener (default 100).</summary>
		public int iTopRows { get; set; } = 100;
	}
}
