using System.Collections.Concurrent;

namespace SantiagoConectaIA.API.Services
{
    public class WhatsAppQueuedMessage
    {
        public string PhoneNumber { get; set; } = string.Empty;
        public string UserMessage { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public DateTime ReceivedAt { get; set; } = DateTime.UtcNow;
        public string WhatsAppMessageId { get; set; } = string.Empty;
    }

    public class WhatsAppMessageQueue
    {
        private readonly ConcurrentQueue<WhatsAppQueuedMessage> _queue = new();
        private readonly ILogger<WhatsAppMessageQueue> _logger;

        public WhatsAppMessageQueue(ILogger<WhatsAppMessageQueue> logger)
        {
            _logger = logger;
        }

        public void Enqueue(WhatsAppQueuedMessage message)
        {
            _queue.Enqueue(message);
            _logger.LogInformation("Message queued from {Phone}: {Message}", message.PhoneNumber, message.UserMessage);
        }

        public bool TryDequeue(out WhatsAppQueuedMessage? message)
        {
            return _queue.TryDequeue(out message);
        }

        public int Count => _queue.Count;
    }
}
