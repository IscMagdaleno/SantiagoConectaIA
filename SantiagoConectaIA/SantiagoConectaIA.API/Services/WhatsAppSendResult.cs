namespace SantiagoConectaIA.API.Services
{
    public class WhatsAppSendResult
    {
        public bool IsSuccess { get; set; }
        public int StatusCode { get; set; }
        public string ResponseBody { get; set; } = string.Empty;
        public string ErrorMessage { get; set; } = string.Empty;
        public string MessageId { get; set; } = string.Empty;
        public string WaId { get; set; } = string.Empty;
        public string Destination { get; set; } = string.Empty;
    }
}
