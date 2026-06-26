namespace SantiagoConectaIA.Share.PostModels.WhatsAppModule
{
    public class WhatsAppConfig
    {
        public string VerifyToken { get; set; } = string.Empty;
        public string AppSecret { get; set; } = string.Empty;
        public string AccessToken { get; set; } = string.Empty;
        public string PhoneNumberId { get; set; } = string.Empty;
        public string ApiVersion { get; set; } = "v25.0";
        public string BaseUrl { get; set; } = "https://graph.facebook.com";
    }
}
