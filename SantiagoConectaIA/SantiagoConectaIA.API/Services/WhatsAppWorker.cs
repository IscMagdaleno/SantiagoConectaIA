using SantiagoConectaIA.API.EngramaLevels.Domain.Interfaces;
using SantiagoConectaIA.Share.PostModels.WhatsAppModule;

namespace SantiagoConectaIA.API.Services
{
    public class WhatsAppWorker : BackgroundService
    {
        private readonly WhatsAppMessageQueue _queue;
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<WhatsAppWorker> _logger;

        public WhatsAppWorker(
            WhatsAppMessageQueue queue,
            IServiceProvider serviceProvider,
            ILogger<WhatsAppWorker> logger)
        {
            _queue = queue;
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("WhatsApp Worker started.");

            while (!stoppingToken.IsCancellationRequested)
            {
                if (_queue.TryDequeue(out var queuedMessage) && queuedMessage != null)
                {
                    await ProcessMessageAsync(queuedMessage, stoppingToken);
                }
                else
                {
                    await Task.Delay(1000, stoppingToken);
                }
            }

            _logger.LogInformation("WhatsApp Worker stopped.");
        }

        private async Task ProcessMessageAsync(WhatsAppQueuedMessage queuedMessage, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Processing message from {Phone}: {Message}",
                    queuedMessage.PhoneNumber, queuedMessage.UserMessage);

                using var scope = _serviceProvider.CreateScope();
                var orchestrationService = scope.ServiceProvider.GetRequiredService<IAgentOrchestrationService>();
                var whatsappService = scope.ServiceProvider.GetRequiredService<IWhatsAppService>();
                var whatsAppDomain = scope.ServiceProvider.GetRequiredService<IWhatsAppDomain>();

                // Guardar mensaje entrante en la base de datos
                int conversationId = 0;
                try
                {
                    // Buscar el usuario por teléfono para obtener su ID
                    var userStats = await whatsAppDomain.GetWhatsAppStats();

                    // Crear o actualizar conversación
                    var conversationResult = await whatsAppDomain.SaveWhatsAppConversation(
                        new PostSaveWhatsAppConversation
                        {
                            iIdConversation = 0,
                            iIdWhatsAppUser = 0, // Se actualizará con el ID real del usuario
                            nvchStatus = "active"
                        });

                    if (conversationResult.IsSuccess && conversationResult.Data != null)
                    {
                        conversationId = conversationResult.Data.iIdConversation;
                    }

                    // Guardar mensaje entrante
                    await whatsAppDomain.SaveWhatsAppMessage(new PostSaveWhatsAppMessage
                    {
                        iIdConversation = conversationId,
                        nvchWhatsAppMessageId = queuedMessage.WhatsAppMessageId,
                        nvchDirection = "inbound",
                        nvchMessageType = "text",
                        nvchContent = queuedMessage.UserMessage,
                        dtTimestamp = queuedMessage.ReceivedAt
                    });
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error saving inbound message for phone {Phone}", queuedMessage.PhoneNumber);
                }

                // Use phone number as the user ID for conversation tracking
                var response = await orchestrationService.ProcessUserQueryAsync(
                    queuedMessage.UserMessage,
                    queuedMessage.PhoneNumber);

                if (response != null && !string.IsNullOrEmpty(response.nvchAgenteResponse))
                {
                    await whatsappService.SendTextMessageAsync(
                        queuedMessage.PhoneNumber,
                        response.nvchAgenteResponse);

                    // Guardar mensaje saliente en la base de datos
                    try
                    {
                        await whatsAppDomain.SaveWhatsAppMessage(new PostSaveWhatsAppMessage
                        {
                            iIdConversation = conversationId,
                            nvchWhatsAppMessageId = "",
                            nvchDirection = "outbound",
                            nvchMessageType = "text",
                            nvchContent = response.nvchAgenteResponse,
                            dtTimestamp = DateTime.UtcNow
                        });
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error saving outbound message for phone {Phone}", queuedMessage.PhoneNumber);
                    }
                }
                else
                {
                    var fallbackMessage = "Lo siento, no pude procesar tu mensaje en este momento. Por favor intenta de nuevo.";
                    await whatsappService.SendTextMessageAsync(
                        queuedMessage.PhoneNumber,
                        fallbackMessage);

                    // Guardar mensaje de fallback
                    try
                    {
                        await whatsAppDomain.SaveWhatsAppMessage(new PostSaveWhatsAppMessage
                        {
                            iIdConversation = conversationId,
                            nvchWhatsAppMessageId = "",
                            nvchDirection = "outbound",
                            nvchMessageType = "text",
                            nvchContent = fallbackMessage,
                            dtTimestamp = DateTime.UtcNow
                        });
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error saving fallback message for phone {Phone}", queuedMessage.PhoneNumber);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing WhatsApp message from {Phone}", queuedMessage.PhoneNumber);

                try
                {
                    using var scope = _serviceProvider.CreateScope();
                    var whatsappService = scope.ServiceProvider.GetRequiredService<IWhatsAppService>();
                    await whatsappService.SendTextMessageAsync(
                        queuedMessage.PhoneNumber,
                        "Ocurrió un error al procesar tu mensaje. Por favor intenta más tarde.");
                }
                catch (Exception sendEx)
                {
                    _logger.LogError(sendEx, "Error sending error response to {Phone}", queuedMessage.PhoneNumber);
                }
            }
        }
    }
}
