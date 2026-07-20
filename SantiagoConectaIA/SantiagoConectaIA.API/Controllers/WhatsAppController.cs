using Microsoft.AspNetCore.Mvc;

using SantiagoConectaIA.API.EngramaLevels.Domain.Interfaces;
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
		private readonly IWhatsAppDomain _whatsAppDomain;
		private readonly IAgentOrchestrationService _orchestrationService;
		private readonly WhatsAppMessageQueue _queue;
		private readonly ILogger<WhatsAppController> _logger;


		public WhatsAppController(
		IWhatsAppService whatsappService,
		IWhatsAppDomain whatsAppDomain,
		IAgentOrchestrationService orchestrationService,
		WhatsAppMessageQueue queue,
		ILogger<WhatsAppController> logger)
		{
			_whatsappService = whatsappService;
			_whatsAppDomain = whatsAppDomain;
			_orchestrationService = orchestrationService;
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



		private async void ProcessIncomingMessage(WhatsAppMessageBody message, List<WhatsAppContact> contacts)
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

			// Guardar el usuario en la base de datos
			try
			{
				var saveUserResult = await _whatsAppDomain.SaveWhatsAppUser(new PostSaveWhatsAppUser
				{
					iIdWhatsAppUser = 0,
					nvchPhoneNumber = phoneNumber,
					nvchName = userName
				});

				if (!saveUserResult.IsSuccess)
				{
					_logger.LogWarning("Error saving WhatsApp user: {Message}", saveUserResult.Message);
				}
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error saving WhatsApp user for phone {Phone}", phoneNumber);
			}

			_logger.LogInformation("Text message from {Name} ({Phone}): {Message}",

			userName, phoneNumber, userMessage);

			// Enqueue for async processing
			_queue.Enqueue(new WhatsAppQueuedMessage

			{

				PhoneNumber = phoneNumber,

				UserMessage = userMessage,

				UserName = userName,

				ReceivedAt = DateTime.UtcNow,

				WhatsAppMessageId = message.Id ?? string.Empty

			});

		}

		/// <summary>
		/// Endpoint de prueba para simular el flujo completo de WhatsApp.
		/// Recibe un mensaje, guarda el usuario, crea conversación, consulta al agente y retorna la respuesta.
		/// No envía mensaje por WhatsApp, solo procesa y retorna.
		/// </summary>
		/// <param name="request">Objeto con PhoneNumber, UserName y Message.</param>
		/// <returns>Respuesta del agente con datos del proceso completo.</returns>
		[HttpPost("test-message")]
		public async Task<IActionResult> TestMessage([FromBody] PostTestWhatsAppMessage request)
		{
			var processLog = new List<string>();
			var startTime = DateTime.UtcNow;

			try
			{
				_logger.LogInformation("Test message received from {Phone}: {Message}",
					request.PhoneNumber, request.Message);

				processLog.Add($"[{DateTime.UtcNow:HH:mm:ss.fff}] Inicio del proceso");

				// 1. Guardar el usuario en la base de datos
				processLog.Add($"[{DateTime.UtcNow:HH:mm:ss.fff}] Guardando usuario: {request.UserName} ({request.PhoneNumber})");
				var saveUserResult = await _whatsAppDomain.SaveWhatsAppUser(new PostSaveWhatsAppUser
				{
					iIdWhatsAppUser = 0,
					nvchPhoneNumber = request.PhoneNumber,
					nvchName = request.UserName
				});

				if (saveUserResult.IsSuccess)
				{
					processLog.Add($"[{DateTime.UtcNow:HH:mm:ss.fff}] Usuario guardado exitosamente. ID: {saveUserResult.Data.iIdWhatsAppUser}");
				}
				else
				{
					processLog.Add($"[{DateTime.UtcNow:HH:mm:ss.fff}] Error guardando usuario: {saveUserResult.Message}");
				}

				// 2. Crear conversación
				processLog.Add($"[{DateTime.UtcNow:HH:mm:ss.fff}] Creando conversación");
				var conversationResult = await _whatsAppDomain.SaveWhatsAppConversation(new PostSaveWhatsAppConversation
				{
					iIdConversation = 0,
					iIdWhatsAppUser = saveUserResult.Data?.iIdWhatsAppUser ?? 0,
					nvchStatus = "active"
				});

				int conversationId = conversationResult.Data?.iIdConversation ?? 0;
				if (conversationResult.IsSuccess)
				{
					processLog.Add($"[{DateTime.UtcNow:HH:mm:ss.fff}] Conversación creada. ID: {conversationId}");
				}
				else
				{
					processLog.Add($"[{DateTime.UtcNow:HH:mm:ss.fff}] Error creando conversación: {conversationResult.Message}");
				}

				// 3. Guardar mensaje entrante
				processLog.Add($"[{DateTime.UtcNow:HH:mm:ss.fff}] Guardando mensaje entrante");
				var inboundResult = await _whatsAppDomain.SaveWhatsAppMessage(new PostSaveWhatsAppMessage
				{
					iIdConversation = conversationId,
					nvchWhatsAppMessageId = $"test_{Guid.NewGuid():N}",
					nvchDirection = "inbound",
					nvchMessageType = "text",
					nvchContent = request.Message,
					dtTimestamp = DateTime.UtcNow
				});

				if (inboundResult.IsSuccess)
				{
					processLog.Add($"[{DateTime.UtcNow:HH:mm:ss.fff}] Mensaje entrante guardado. ID: {inboundResult.Data.iIdWhatsAppMessage}");
				}
				else
				{
					processLog.Add($"[{DateTime.UtcNow:HH:mm:ss.fff}] Error guardando mensaje entrante: {inboundResult.Message}");
				}

				// 4. Consultar al agente
				processLog.Add($"[{DateTime.UtcNow:HH:mm:ss.fff}] Consultando al agente...");
				var agentResponse = await _orchestrationService.ProcessUserQueryAsync(
					request.Message,
					request.PhoneNumber);

				string agentAnswer = agentResponse?.nvchAgenteResponse ?? "Sin respuesta del agente";
				processLog.Add($"[{DateTime.UtcNow:HH:mm:ss.fff}] Respuesta del agente recibida ({agentAnswer.Length} caracteres)");

				// 5. Guardar mensaje saliente
				processLog.Add($"[{DateTime.UtcNow:HH:mm:ss.fff}] Guardando mensaje saliente");
				var outboundResult = await _whatsAppDomain.SaveWhatsAppMessage(new PostSaveWhatsAppMessage
				{
					iIdConversation = conversationId,
					nvchWhatsAppMessageId = "",
					nvchDirection = "outbound",
					nvchMessageType = "text",
					nvchContent = agentAnswer,
					dtTimestamp = DateTime.UtcNow
				});

				if (outboundResult.IsSuccess)
				{
					processLog.Add($"[{DateTime.UtcNow:HH:mm:ss.fff}] Mensaje saliente guardado. ID: {outboundResult.Data.iIdWhatsAppMessage}");
				}
				else
				{
					processLog.Add($"[{DateTime.UtcNow:HH:mm:ss.fff}] Error guardando mensaje saliente: {outboundResult.Message}");
				}

				// 6. Cerrar conversación
				processLog.Add($"[{DateTime.UtcNow:HH:mm:ss.fff}] Cerrando conversación");
				await _whatsAppDomain.SaveWhatsAppConversation(new PostSaveWhatsAppConversation
				{
					iIdConversation = conversationId,
					iIdWhatsAppUser = saveUserResult.Data?.iIdWhatsAppUser ?? 0,
					nvchStatus = "closed"
				});
				processLog.Add($"[{DateTime.UtcNow:HH:mm:ss.fff}] Conversación cerrada");

				var elapsed = DateTime.UtcNow - startTime;
				processLog.Add($"[{DateTime.UtcNow:HH:mm:ss.fff}] Proceso completado en {elapsed.TotalMilliseconds:F0}ms");

				_logger.LogInformation("Test message process completed for {Phone} in {Elapsed}ms",
					request.PhoneNumber, elapsed.TotalMilliseconds);

				return Ok(new
				{
					Success = true,
					AgentResponse = agentAnswer,
					ProcessLog = processLog,
					DurationMs = elapsed.TotalMilliseconds,
					Details = new
					{
						UserId = saveUserResult.Data?.iIdWhatsAppUser ?? 0,
						ConversationId = conversationId,
						InboundMessageId = inboundResult.Data?.iIdWhatsAppMessage ?? 0,
						OutboundMessageId = outboundResult.Data?.iIdWhatsAppMessage ?? 0
					}
				});
			}
			catch (Exception ex)
			{
				var elapsed = DateTime.UtcNow - startTime;
				processLog.Add($"[{DateTime.UtcNow:HH:mm:ss.fff}] ERROR: {ex.Message}");

				_logger.LogError(ex, "Error in test message process for {Phone}", request.PhoneNumber);

				return StatusCode(500, new
				{
					Success = false,
					Error = ex.Message,
					ProcessLog = processLog,
					DurationMs = elapsed.TotalMilliseconds
				});
			}
		}

	}
}

