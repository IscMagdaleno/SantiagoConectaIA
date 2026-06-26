using System.Text.Json.Serialization;

namespace SantiagoConectaIA.Share.Objects.WhatsAppModule
{
    public class WhatsAppInboundMessage
    {
        [JsonPropertyName("object")]
        public string Object { get; set; } = string.Empty;

        [JsonPropertyName("entry")]
        public List<WhatsAppEntry> Entry { get; set; } = new();
    }

    public class WhatsAppEntry
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;

        [JsonPropertyName("changes")]
        public List<WhatsAppChange> Changes { get; set; } = new();
    }

    public class WhatsAppChange
    {
        [JsonPropertyName("field")]
        public string Field { get; set; } = string.Empty;

        [JsonPropertyName("value")]
        public WhatsAppChangeValue Value { get; set; } = new();
    }

    public class WhatsAppChangeValue
    {
        [JsonPropertyName("messaging_product")]
        public string MessagingProduct { get; set; } = string.Empty;

        [JsonPropertyName("metadata")]
        public WhatsAppMetadata Metadata { get; set; } = new();

        [JsonPropertyName("contacts")]
        public List<WhatsAppContact> Contacts { get; set; } = new();

        [JsonPropertyName("messages")]
        public List<WhatsAppMessageBody> Messages { get; set; } = new();
    }

    public class WhatsAppMetadata
    {
        [JsonPropertyName("display_phone_number")]
        public string DisplayPhoneNumber { get; set; } = string.Empty;

        [JsonPropertyName("phone_number_id")]
        public string PhoneNumberId { get; set; } = string.Empty;
    }

    public class WhatsAppContact
    {
        [JsonPropertyName("profile")]
        public WhatsAppProfile Profile { get; set; } = new();

        [JsonPropertyName("wa_id")]
        public string WaId { get; set; } = string.Empty;
    }

    public class WhatsAppProfile
    {
        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;
    }

    public class WhatsAppMessageBody
    {
        [JsonPropertyName("from")]
        public string From { get; set; } = string.Empty;

        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;

        [JsonPropertyName("timestamp")]
        public string Timestamp { get; set; } = string.Empty;

        [JsonPropertyName("type")]
        public string Type { get; set; } = string.Empty;

        [JsonPropertyName("text")]
        public WhatsAppTextBody? Text { get; set; }
    }

    public class WhatsAppTextBody
    {
        [JsonPropertyName("body")]
        public string Body { get; set; } = string.Empty;
    }
}
