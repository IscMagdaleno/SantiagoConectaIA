using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

using SantiagoConectaIA.API.Services;
using SantiagoConectaIA.Share.Objects.WhatsAppModule;

using System.Text;
using System.Text.Json;

namespace SantiagoConectaIA.API.Controllers
{
	/// <summary>
	/// Webhook de WhatsApp Cloud API. Solo valida la petición, normaliza el teléfono y encola el mensaje;
	/// el guardado en base de datos y la respuesta del agente los hace <see cref="WhatsAppWorker"/>.
	/// </summary>
	[ApiController]
	[Route("api/[controller]")]
	public class WhatsAppController : ControllerBase
	{
		private readonly IWhatsAppService _whatsappService;
		private readonly WhatsAppMessageQueue _queue;
		private readonly IConfiguration _configuration;
		private readonly ILogger<WhatsAppController> _logger;

		public WhatsAppController(
			IWhatsAppService whatsappService,
			WhatsAppMessageQueue queue,
			IConfiguration configuration,
			ILogger<WhatsAppController> logger)
		{
			_whatsappService = whatsappService;
			_queue = queue;
			_configuration = configuration;
			_logger = logger;
		}

		/// <summary>
		/// Verificación del webhook: Meta manda un GET para comprobar el token.
		/// </summary>
		[HttpGet("webhook")]
		[DisableRateLimiting]
		public IActionResult VerifyWebhook(
			[FromQuery(Name = "hub.mode")] string mode,
			[FromQuery(Name = "hub.verify_token")] string token,
			[FromQuery(Name = "hub.challenge")] string challenge)
		{
			if (mode == "subscribe" && _whatsappService.VerifyWebhookToken(token))
			{
				_logger.LogInformation("Webhook de WhatsApp verificado.");
				return Content(challenge, "text/plain");
			}

			_logger.LogWarning("Falló la verificación del webhook de WhatsApp (mode={Mode}).", mode);
			return Forbid();
		}

		/// <summary>
		/// Recibe los mensajes de Meta. Exige la firma <c>X-Hub-Signature-256</c> calculada con el App Secret
		/// (se puede desactivar solo en una emergencia con la configuración <c>WhatsApp:ValidarFirma=false</c>)
		/// y responde 200 de inmediato para que Meta no reintente.
		/// </summary>
		[HttpPost("webhook")]
		[DisableRateLimiting]
		public async Task<IActionResult> ReceiveMessage(CancellationToken cancellationToken)
		{
			try
			{
				Request.EnableBuffering();
				using var reader = new StreamReader(Request.Body, Encoding.UTF8, detectEncodingFromByteOrderMarks: false, bufferSize: 1024, leaveOpen: true);
				var rawBody = await reader.ReadToEndAsync(cancellationToken);
				Request.Body.Position = 0;

				if (_configuration.GetValue("WhatsApp:ValidarFirma", true))
				{
					var firma = Request.Headers["X-Hub-Signature-256"].FirstOrDefault() ?? string.Empty;
					if (!_whatsappService.ValidateSignature(rawBody, firma))
					{
						_logger.LogWarning("Webhook de WhatsApp rechazado: firma inválida o ausente.");
						return Unauthorized();
					}
				}

				var payload = JsonSerializer.Deserialize<WhatsAppInboundMessage>(rawBody);
				if (payload == null || payload.Object != "whatsapp_business_account")
				{
					return Ok();
				}

				foreach (var change in payload.Entry.SelectMany(entry => entry.Changes).Where(change => change.Field == "messages"))
				{
					// Sin contactos es una actualización de estado o un mensaje de grupo.
					if (change.Value.Contacts.Count == 0)
					{
						continue;
					}

					var nombre = change.Value.Contacts[0].Profile.Name;
					foreach (var message in change.Value.Messages)
					{
						EncolarMensaje(message, string.IsNullOrWhiteSpace(nombre) ? "Usuario" : nombre);
					}
				}
			}
			catch (Exception ex)
			{
				// Aun con error se responde 200 para evitar reintentos de Meta.
				_logger.LogError(ex, "Error al procesar el webhook de WhatsApp.");
			}

			return Ok();
		}

		private void EncolarMensaje(WhatsAppMessageBody message, string nombreUsuario)
		{
			if (message.Type != "text" || message.Text == null)
			{
				_logger.LogInformation("Se ignora un mensaje de WhatsApp de tipo {Type}.", message.Type);
				return;
			}

			_queue.Enqueue(new WhatsAppQueuedMessage
			{
				PhoneNumber = NormalizarTelefonoMexico(message.From),
				UserMessage = message.Text.Body.Trim(),
				UserName = nombreUsuario,
				ReceivedAt = DateTime.UtcNow,
				WhatsAppMessageId = message.Id
			});
		}

		/// <summary>
		/// Los números de México llegan como 521XXXXXXXXXX (13 dígitos); la API de envío espera 52XXXXXXXXXX.
		/// </summary>
		private static string NormalizarTelefonoMexico(string telefono) =>
			telefono.StartsWith("521") && telefono.Length == 13 ? "52" + telefono[3..] : telefono;
	}
}
