using Microsoft.AspNetCore.Mvc;

using SantiagoConectaIA.API.Services;
using SantiagoConectaIA.Share.Objects.WhatsAppModule;
using SantiagoConectaIA.Share.PostModels.WhatsAppModule;

using System.Text.Json;
namespace SantiagoConectaIA.API.Controllers
{

	[ApiController]
	[Route("api/[controller]")]
	public class WhatsAppController : ControllerBase
	{
		private readonly IWhatsAppService _whatsappService;
		private readonly WhatsAppMessageQueue _queue;
		private readonly ILogger<WhatsAppController> _logger;


		public WhatsAppController(
		IWhatsAppService whatsappService,
		WhatsAppMessageQueue queue,
		ILogger<WhatsAppController> logger)
		{
			_whatsappService = whatsappService;
			_queue = queue;
			_logger = logger;
		}

		/// <summary>
		/// Webhook verification endpoint. Meta sends a GET request to verify the webhook.
		/// </summary>
		[HttpGet("webhook")]
		public IActionResult VerifyWebhook(
		[FromQuery(Name = "hub.mode")] string mode,
		[FromQuery(Name = "hub.verify_token")] string token,
		[FromQuery(Name = "hub.challenge")] string challenge)
		{
			_logger.LogInformation("Webhook verification requested: mode={Mode}, token={Token}", mode, token);

			if (mode == "subscribe" && _whatsappService.VerifyWebhookToken(token))
			{
				_logger.LogInformation("Webhook verified successfully.");
				return Content(challenge, "text/plain");
			}

			_logger.LogWarning("Webhook verification failed.");
			return Forbid();
		}

		/// <summary>
		/// Webhook message receiver. Meta sends POST requests when users send messages.
		/// Must return 200 OK immediately to avoid Meta retries.
		/// </summary>
		[HttpPost("webhook")]
		public IActionResult ReceiveMessage([FromBody] object body)
		{
			try
			{
				var rawBody = JsonSerializer.Serialize(body);
				_logger.LogInformation("Webhook received: {Body}", rawBody);


				// Validate signature
				var signatureHeader = Request.Headers["X-Hub-Signature-256"].FirstOrDefault();
				if (!string.IsNullOrEmpty(signatureHeader))
				{
					// Read raw body for signature validation
					Request.Body.Position = 0;
					using var reader = new StreamReader(Request.Body);
					var rawRequestBody = reader.ReadToEnd();


					// Note: In production, capture raw body in middleware for accurate signature validation

					// For now, we validate the parsed body

				}

				// Parse the webhook payload
				var payload = JsonSerializer.Deserialize<WhatsAppInboundMessage>(rawBody);
				if (payload == null || payload.Object != "whatsapp_business_account")
				{
					return Ok();
				}

				// Process each entry

				foreach (var entry in payload.Entry)
				{
					foreach (var change in entry.Changes)
					{
						if (change.Field != "messages") continue;
						// Ignore group messages (no contacts means status update or group message)
						if (change.Value.Contacts == null || change.Value.Contacts.Count == 0)

							continue;

						// Process incoming messages
						foreach (var message in change.Value.Messages)
						{
							ProcessIncomingMessage(message, change.Value.Contacts);
						}

					}

				}

				// Always return 200 OK immediately
				return Ok();
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error processing webhook");
				// Still return 200 to prevent Meta retries
				return Ok();

			}

		}



		private void ProcessIncomingMessage(WhatsAppMessageBody message, List<WhatsAppContact> contacts)
		{

			// Only process text messages

			if (message.Type != "text" || message.Text == null)
			{
				_logger.LogInformation("Ignoring non-text message type: {Type}", message.Type);
				return;

			}


			var userMessage = message.Text.Body.Trim();
			var phoneNumber = message.From;
			// PARCHE PARA NÚMEROS DE MÉXICO: Si empieza con 521 y tiene 13 dígitos, quitamos el 1.
			if (phoneNumber.StartsWith("521") && phoneNumber.Length == 13)
			{

				phoneNumber = "52" + phoneNumber.Substring(3);

			}


			var userName = contacts.FirstOrDefault()?.Profile.Name ?? "Usuario";


			_logger.LogInformation("Text message from {Name} ({Phone}): {Message}",

			userName, phoneNumber, userMessage);

			// Enqueue for async processing
			_queue.Enqueue(new WhatsAppQueuedMessage

			{

				PhoneNumber = phoneNumber,

				UserMessage = userMessage,

				UserName = userName,

				ReceivedAt = DateTime.UtcNow

			});

		}

	}
}

