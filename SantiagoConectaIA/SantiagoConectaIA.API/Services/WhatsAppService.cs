using SantiagoConectaIA.API.EngramaLevels.Domain.Interfaces;
using SantiagoConectaIA.Share.PostModels.CatalogosModule;
using SantiagoConectaIA.Share.PostModels.WhatsAppModule;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace SantiagoConectaIA.API.Services
{
    public class WhatsAppService : IWhatsAppService
    {
        private readonly HttpClient _httpClient;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<WhatsAppService> _logger;
        private WhatsAppConfig? _configCache;

        public WhatsAppService(
            HttpClient httpClient,
            IServiceScopeFactory scopeFactory,
            ILogger<WhatsAppService> logger)
        {
            _httpClient = httpClient;
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        private WhatsAppConfig GetConfig()
        {
            if (_configCache != null) return _configCache;

            var config = new WhatsAppConfig();
            using (var scope = _scopeFactory.CreateScope())
            {
                var catalogosDomain = scope.ServiceProvider.GetRequiredService<ICatalogosDomain>();
                
                var param1 = catalogosDomain.GetParametroByAlias(new PostGetParametro { vchAlias = "WHATSAPP_CONFIG_1" }).GetAwaiter().GetResult();
                if (param1 != null && param1.IsSuccess && param1.Data != null)
                {
                    config.VerifyToken = param1.Data.NvchValor1 ?? "";
                    config.AppSecret = param1.Data.NvchValor2 ?? "";
                }

                var param2 = catalogosDomain.GetParametroByAlias(new PostGetParametro { vchAlias = "WHATSAPP_CONFIG_2" }).GetAwaiter().GetResult();
                if (param2 != null && param2.IsSuccess && param2.Data != null)
                {
                    config.AccessToken = param2.Data.NvchValor1 ?? "";
                    config.PhoneNumberId = param2.Data.NvchValor2 ?? "";
                }

                var param3 = catalogosDomain.GetParametroByAlias(new PostGetParametro { vchAlias = "WHATSAPP_CONFIG_3" }).GetAwaiter().GetResult();
                if (param3 != null && param3.IsSuccess && param3.Data != null)
                {
                    config.ApiVersion = param3.Data.NvchValor1 ?? "v25.0";
                    config.BaseUrl = param3.Data.NvchValor2 ?? "https://graph.facebook.com";
                }
            }

            _configCache = config;
            return _configCache;
        }

        public async Task<bool> SendTextMessageAsync(string to, string message)
        {
            try
            {
                var config = GetConfig();
                var url = $"{config.BaseUrl}/{config.ApiVersion}/{config.PhoneNumberId}/messages";

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
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", config.AccessToken);

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
            var config = GetConfig();
            if (string.IsNullOrEmpty(signatureHeader) || string.IsNullOrEmpty(config.AppSecret))
                return false;

            try
            {
                // signatureHeader format: "sha256=<hex-digest>"
                var receivedHash = signatureHeader.Replace("sha256=", "");

                using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(config.AppSecret));
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

        public bool VerifyWebhookToken(string token)
        {
            var config = GetConfig();
            return token == config.VerifyToken;
        }
    }
}
