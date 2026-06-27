namespace SantiagoConectaIA.API.Services
{
    public interface IWhatsAppService
    {
        Task<bool> SendTextMessageAsync(string to, string message);
        bool ValidateSignature(string body, string signatureHeader);
        bool VerifyWebhookToken(string token);
    }
}
