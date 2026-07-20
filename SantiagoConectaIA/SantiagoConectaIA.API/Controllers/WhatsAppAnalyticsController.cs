using Microsoft.AspNetCore.Mvc;

using SantiagoConectaIA.API.EngramaLevels.Domain.Interfaces;
using SantiagoConectaIA.Share.PostModels.WhatsAppModule;

namespace SantiagoConectaIA.API.Controllers
{
	/// <summary>
	/// Controlador para estadísticas y analíticas del módulo de WhatsApp.
	/// </summary>
	[ApiController]
	[Route("api/[controller]")]
	public class WhatsAppAnalyticsController : ControllerBase
	{
		private readonly IWhatsAppDomain _whatsAppDomain;
		private readonly ILogger<WhatsAppAnalyticsController> _logger;

		/// <summary>
		/// Inicializa el controlador con la dependencia de la capa de Dominio.
		/// </summary>
		/// <param name="whatsAppDomain">Interfaz de la capa de Dominio para el módulo WhatsApp.</param>
		/// <param name="logger">Logger para registro de eventos.</param>
		public WhatsAppAnalyticsController(
			IWhatsAppDomain whatsAppDomain,
			ILogger<WhatsAppAnalyticsController> logger)
		{
			_whatsAppDomain = whatsAppDomain;
			_logger = logger;
		}

		/// <summary>
		/// Obtiene estadísticas generales del módulo de WhatsApp.
		/// </summary>
		/// <returns>Respuesta con las estadísticas generales o un mensaje de error.</returns>
		[HttpGet("stats")]
		public async Task<IActionResult> GetStats()
		{
			_logger.LogInformation("Consultando estadísticas de WhatsApp");
			var result = await _whatsAppDomain.GetWhatsAppStats();
			if (result.IsSuccess)
			{
				return Ok(result);
			}
			return BadRequest(result);
		}

		/// <summary>
		/// Obtiene estadísticas diarias de WhatsApp para gráficas.
		/// </summary>
		/// <param name="days">Número de días hacia atrás a consultar (default: 30).</param>
		/// <returns>Respuesta con las estadísticas diarias o un mensaje de error.</returns>
		[HttpGet("daily")]
		public async Task<IActionResult> GetDailyStats([FromQuery] int days = 30)
		{
			_logger.LogInformation("Consultando estadísticas diarias de WhatsApp para los últimos {Days} días", days);
			var result = await _whatsAppDomain.GetWhatsAppDailyStats(
				new PostGetWhatsAppDailyStats { iDays = days });
			if (result.IsSuccess)
			{
				return Ok(result);
			}
			return BadRequest(result);
		}
	}
}
