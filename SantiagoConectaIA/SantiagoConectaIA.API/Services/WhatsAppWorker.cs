using SantiagoConectaIA.API.EngramaLevels.Domain.Interfaces;

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

                // Use phone number as the user ID for conversation tracking
                var response = await orchestrationService.ProcessUserQueryAsync(
                    queuedMessage.UserMessage,
                    queuedMessage.PhoneNumber);

                if (response != null && !string.IsNullOrEmpty(response.nvchAgenteResponse))
                {
                    await whatsappService.SendTextMessageAsync(
                        queuedMessage.PhoneNumber,
                        response.nvchAgenteResponse);
                }
                else
                {
                    await whatsappService.SendTextMessageAsync(
                        queuedMessage.PhoneNumber,
                        "Lo siento, no pude procesar tu mensaje en este momento. Por favor intenta de nuevo.");
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
