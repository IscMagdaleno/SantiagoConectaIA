using System.Text;
using System.Text.Json;
using EngramaCoreStandar.Results;
using SantiagoConectaIA.Share.PostModels.PublicacionesModule;

namespace SantiagoConectaIA.API.Services
{
    public class FacebookPublishService : IFacebookPublishService
    {
        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNamingPolicy = null
        };

        private readonly HttpClient _httpClient;
        private readonly IParametrosService _parametros;
        private readonly ILogger<FacebookPublishService> _logger;

        public FacebookPublishService(
            HttpClient httpClient,
            IParametrosService parametros,
            ILogger<FacebookPublishService> logger)
        {
            _httpClient = httpClient;
            _parametros = parametros;
            _logger = logger;
        }

        public async Task<Response<string>> PublicarAsync(PostPublicarFacebook post, CancellationToken cancellationToken = default)
        {
            if (post == null || string.IsNullOrWhiteSpace(post.Message) || string.IsNullOrWhiteSpace(post.ImageUrl))
            {
                return Response<string>.BadResult("El texto y la URL de la imagen son obligatorios.", string.Empty);
            }

            if (!Uri.TryCreate(post.ImageUrl.Trim(), UriKind.Absolute, out var imageUri)
                || (imageUri.Scheme != Uri.UriSchemeHttp && imageUri.Scheme != Uri.UriSchemeHttps))
            {
                return Response<string>.BadResult("La imagen de portada debe ser una URL http o https.", string.Empty);
            }

            var webhookUrl = await _parametros.GetValor1Async(ParametrosAlias.MakeFacebookWebhook, cancellationToken);
            if (string.IsNullOrWhiteSpace(webhookUrl))
            {
                return Response<string>.BadResult($"No está configurada la URL del webhook de Make (parámetro {ParametrosAlias.MakeFacebookWebhook}).", string.Empty);
            }

            var payload = new PostPublicarFacebook
            {
                Message = post.Message.Trim(),
                ImageUrl = imageUri.ToString()
            };

            try
            {
                var json = JsonSerializer.Serialize(payload, JsonOptions);
                using var content = new StringContent(json, Encoding.UTF8, "application/json");
                using var response = await _httpClient.PostAsync(webhookUrl, content, cancellationToken);
                var body = await response.Content.ReadAsStringAsync(cancellationToken);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("Make rechazó la publicación. Status={StatusCode}", (int)response.StatusCode);
                    return Response<string>.BadResult($"Make respondió {(int)response.StatusCode}.", string.Empty);
                }

                return new Response<string>
                {
                    IsSuccess = true,
                    Data = string.IsNullOrWhiteSpace(body) ? "Accepted" : body,
                    Message = "Publicación enviada a Facebook."
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al enviar la publicación al webhook de Make.");
                return Response<string>.BadResult("No se pudo contactar el webhook de Make.", string.Empty);
            }
        }
    }
}
