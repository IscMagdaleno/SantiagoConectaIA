using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using EngramaCoreStandar.Results;
using SantiagoConectaIA.API.EngramaLevels.Domain.Interfaces;
using SantiagoConectaIA.Share.PostModels.PublicacionesModule;

namespace SantiagoConectaIA.API.Services
{
    public class GeminiImagenService : IGeminiImagenService
    {
        private const string ModeloDefault = "gemini-3.1-flash-image";
        private const string Contenedor = "publicaciones";
        private const int MaxCaracteresPrompt = 2000;
        private const long MaxBytesImagen = 15 * 1024 * 1024;

        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };

        private readonly HttpClient _httpClient;
        private readonly IParametrosService _parametros;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<GeminiImagenService> _logger;

        public GeminiImagenService(
            HttpClient httpClient,
            IParametrosService parametros,
            IServiceScopeFactory scopeFactory,
            ILogger<GeminiImagenService> logger)
        {
            _httpClient = httpClient;
            _parametros = parametros;
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        public async Task<Response<string>> EditarAsync(PostEditarImagen post, CancellationToken cancellationToken = default)
        {
            var prompt = post?.vchPrompt?.Trim() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(prompt))
            {
                return Response<string>.BadResult("Escribe o elige un prompt para editar la imagen.", string.Empty);
            }

            if (prompt.Length > MaxCaracteresPrompt)
            {
                return Response<string>.BadResult($"El prompt no puede pasar de {MaxCaracteresPrompt} caracteres.", string.Empty);
            }

            if (!Uri.TryCreate(post!.vchImagenUrl?.Trim(), UriKind.Absolute, out var imagenUri)
                || (imagenUri.Scheme != Uri.UriSchemeHttp && imagenUri.Scheme != Uri.UriSchemeHttps))
            {
                return Response<string>.BadResult("La imagen a editar debe ser una URL http o https.", string.Empty);
            }

            var apiKey = await _parametros.GetValor2Async(ParametrosAlias.Gemini, cancellationToken);
            if (string.IsNullOrWhiteSpace(apiKey))
            {
                return Response<string>.BadResult($"No está configurada la clave de Gemini (parámetro {ParametrosAlias.Gemini}).", string.Empty);
            }

            var modelo = await _parametros.GetValor1Async(ParametrosAlias.GeminiImagen, cancellationToken) ?? ModeloDefault;
            if (modelo.StartsWith("models/", StringComparison.OrdinalIgnoreCase))
            {
                modelo = modelo["models/".Length..];
            }

            try
            {
                var original = await DescargarAsync(imagenUri, cancellationToken);
                if (!original.IsSuccess)
                {
                    return Response<string>.BadResult(original.Message, string.Empty);
                }

                var editada = await GenerarAsync(modelo, apiKey, prompt, original.Data, cancellationToken);
                if (!editada.IsSuccess)
                {
                    return Response<string>.BadResult(editada.Message, string.Empty);
                }

                return await GuardarAsync(editada.Data);
            }
            catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
            {
                return Response<string>.BadResult("Gemini tardó demasiado en editar la imagen. Inténtalo de nuevo.", string.Empty);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al editar la imagen con Gemini.");
                return Response<string>.BadResult("No se pudo editar la imagen con Gemini.", string.Empty);
            }
        }

        private async Task<Response<Imagen>> DescargarAsync(Uri uri, CancellationToken cancellationToken)
        {
            using var response = await _httpClient.GetAsync(uri, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                return Response<Imagen>.BadResult($"No se pudo descargar la imagen original ({(int)response.StatusCode}).", new Imagen());
            }

            if (response.Content.Headers.ContentLength > MaxBytesImagen)
            {
                return Response<Imagen>.BadResult("La imagen original pesa más de 15 MB.", new Imagen());
            }

            await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
            using var memoria = new MemoryStream();
            var buffer = new byte[81920];
            int leidos;
            while ((leidos = await stream.ReadAsync(buffer, cancellationToken)) > 0)
            {
                if (memoria.Length + leidos > MaxBytesImagen)
                {
                    return Response<Imagen>.BadResult("La imagen original pesa más de 15 MB.", new Imagen());
                }

                memoria.Write(buffer, 0, leidos);
            }

            var bytes = memoria.ToArray();
            var formato = FormatoArchivo.Detectar(bytes);
            if (formato is not { EsImagen: true } || formato == FormatoArchivo.Gif)
            {
                var detalle = formato != null ? $" (es {formato.Extension.ToUpperInvariant()})" : string.Empty;
                return Response<Imagen>.BadResult($"El archivo no es una imagen PNG, JPG, WEBP o HEIC{detalle}.", new Imagen());
            }

            return new Response<Imagen>
            {
                IsSuccess = true,
                Data = new Imagen { MimeType = formato.MimeType, Bytes = bytes }
            };
        }

        private async Task<Response<Imagen>> GenerarAsync(string modelo, string apiKey, string prompt, Imagen original, CancellationToken cancellationToken)
        {
            var requestBody = new GeminiRequest
            {
                Contents = new List<GeminiContent>
                {
                    new()
                    {
                        Role = "user",
                        Parts = new List<GeminiPart>
                        {
                            new() { Text = prompt },
                            new() { InlineData = new GeminiInlineData { MimeType = original.MimeType, Data = Convert.ToBase64String(original.Bytes) } }
                        }
                    }
                },
                GenerationConfig = new GeminiGenerationConfig { ResponseModalities = new List<string> { "IMAGE" } }
            };

            var url = $"https://generativelanguage.googleapis.com/v1beta/models/{Uri.EscapeDataString(modelo)}:generateContent?key={Uri.EscapeDataString(apiKey)}";
            var json = JsonSerializer.Serialize(requestBody, JsonOptions);
            using var content = new StringContent(json, Encoding.UTF8, "application/json");
            using var response = await _httpClient.PostAsync(url, content, cancellationToken);
            var body = await response.Content.ReadAsStringAsync(cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Gemini rechazó la edición de imagen. Status={StatusCode}", (int)response.StatusCode);
                return Response<Imagen>.BadResult($"Gemini no pudo editar la imagen ({(int)response.StatusCode}).", new Imagen());
            }

            var parsed = JsonSerializer.Deserialize<GeminiResponse>(body, JsonOptions);
            var partes = parsed?.Candidates?
                .SelectMany(c => c.Content?.Parts ?? new List<GeminiPart>())
                .ToList() ?? new List<GeminiPart>();

            var imagen = partes.FirstOrDefault(p => !string.IsNullOrWhiteSpace(p.InlineData?.Data))?.InlineData;
            if (imagen == null)
            {
                var texto = partes.Select(p => p.Text).FirstOrDefault(t => !string.IsNullOrWhiteSpace(t));
                var motivo = parsed?.Candidates?.Select(c => c.FinishReason).FirstOrDefault(r => !string.IsNullOrWhiteSpace(r));
                var mensaje = !string.IsNullOrWhiteSpace(texto)
                    ? $"Gemini no devolvió imagen: {texto.Trim()}"
                    : $"Gemini no devolvió imagen{(string.IsNullOrWhiteSpace(motivo) ? "." : $" (motivo: {motivo}).")}";
                return Response<Imagen>.BadResult(mensaje, new Imagen());
            }

            return new Response<Imagen>
            {
                IsSuccess = true,
                Data = new Imagen
                {
                    MimeType = string.IsNullOrWhiteSpace(imagen.MimeType) ? "image/png" : imagen.MimeType,
                    Bytes = Convert.FromBase64String(imagen.Data!)
                }
            };
        }

        private async Task<Response<string>> GuardarAsync(Imagen imagen)
        {
            var extension = imagen.MimeType.ToLowerInvariant() switch
            {
                "image/jpeg" or "image/jpg" => "jpg",
                "image/webp" => "webp",
                _ => "png"
            };
            var nombre = $"ia-{DateTime.UtcNow:yyyyMMddHHmmss}-{Guid.NewGuid():N}.{extension}";

            using var scope = _scopeFactory.CreateScope();
            var blobDomain = scope.ServiceProvider.GetRequiredService<IAzureBlobDomain>();
            using var stream = new MemoryStream(imagen.Bytes);
            var subida = await blobDomain.UploadDocument(stream, nombre, Contenedor);

            if (!subida.IsSuccess || string.IsNullOrWhiteSpace(subida.Data?.URL))
            {
                return Response<string>.BadResult(
                    string.IsNullOrWhiteSpace(subida.Message) ? "No se pudo guardar la imagen editada." : subida.Message,
                    string.Empty);
            }

            return new Response<string>
            {
                IsSuccess = true,
                Data = subida.Data.URL,
                Message = "Imagen editada con IA."
            };
        }

        private sealed class Imagen
        {
            public string MimeType { get; set; } = string.Empty;
            public byte[] Bytes { get; set; } = Array.Empty<byte>();
        }

        private sealed class GeminiRequest
        {
            public List<GeminiContent> Contents { get; set; } = new();
            public GeminiGenerationConfig? GenerationConfig { get; set; }
        }

        private sealed class GeminiContent
        {
            public string? Role { get; set; }
            public List<GeminiPart> Parts { get; set; } = new();
        }

        private sealed class GeminiPart
        {
            public string? Text { get; set; }
            public GeminiInlineData? InlineData { get; set; }
        }

        private sealed class GeminiInlineData
        {
            public string? MimeType { get; set; }
            public string? Data { get; set; }
        }

        private sealed class GeminiGenerationConfig
        {
            public List<string> ResponseModalities { get; set; } = new();
        }

        private sealed class GeminiResponse
        {
            public List<GeminiCandidate>? Candidates { get; set; }
        }

        private sealed class GeminiCandidate
        {
            public GeminiContent? Content { get; set; }
            public string? FinishReason { get; set; }
        }
    }
}
