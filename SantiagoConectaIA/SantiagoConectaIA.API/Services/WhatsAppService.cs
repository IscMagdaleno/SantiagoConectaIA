using SantiagoConectaIA.Share.PostModels.WhatsAppModule;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace SantiagoConectaIA.API.Services
{
    public class WhatsAppService : IWhatsAppService
    {
        private readonly HttpClient _httpClient;
        private readonly WhatsAppConfig _config;
        private readonly ILogger<WhatsAppService> _logger;

        public WhatsAppService(
            HttpClient httpClient,
            WhatsAppConfig config,
            ILogger<WhatsAppService> logger)
        {
            _httpClient = httpClient;
            _config = config;
            _logger = logger;
        }

        public async Task<bool> SendTextMessageAsync(string to, string message)
        {
            try
            {
                var url = $"{_config.BaseUrl}/{_config.ApiVersion}/{_config.PhoneNumberId}/messages";

                var payload = new
                {
                    messaging_product = "whatsapp",
                    recipient_type = "individual",
                    to = to,
                    type = "text",
                    text = new { body = message }
                };

                var json = JsonSerializer.Serialize(payload);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                _httpClient.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _config.AccessToken);

                var response = await _httpClient.PostAsync(url, content);
                var responseText = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation("WhatsApp message sent to {To}", to);
                    return true;
                }

                _logger.LogWarning("WhatsApp API returned {StatusCode}: {Response}", response.StatusCode, responseText);
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending WhatsApp message to {To}", to);
                return false;
            }
        }

        public bool ValidateSignature(string body, string signatureHeader)
        {
            if (string.IsNullOrEmpty(signatureHeader) || string.IsNullOrEmpty(_config.AppSecret))
                return false;

            try
            {
                // signatureHeader format: "sha256=<hex-digest>"
                var receivedHash = signatureHeader.Replace("sha256=", "");

                using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(_config.AppSecret));
                var computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(body));
                var computedHex = BitConverter.ToString(computedHash).Replace("-", "").ToLower();

                return string.Equals(receivedHash, computedHex, StringComparison.OrdinalIgnoreCase);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating WhatsApp signature");
                return false;
            }
        }
    }
}
