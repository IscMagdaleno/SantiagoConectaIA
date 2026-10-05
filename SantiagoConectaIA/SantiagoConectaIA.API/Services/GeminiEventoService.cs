using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using EngramaCoreStandar.Results;
using SantiagoConectaIA.Share.Objects.EventosModulo;

namespace SantiagoConectaIA.API.Services
{
    public class GeminiEventoService : IGeminiEventoService
    {
        private const string ModeloDefault = "gemini-2.5-flash";
        private readonly HttpClient _httpClient;
        private readonly IParametrosService _parametros;
        private readonly ILogger<GeminiEventoService> _logger;

        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNameCaseInsensitive = true,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };

        public GeminiEventoService(HttpClient httpClient,IParametrosService parametros,ILogger<GeminiEventoService> logger)
        {
            _httpClient = httpClient;
            _parametros = parametros;
            _logger = logger;
        }

        public async Task<Response<EventoExtraidoDto>> ExtraerEventoDesdeImagenAsync(Stream imageStream, string mimeType, string imageUrl, CancellationToken cancellationToken = default)
        {
            try
            {
                var apiKey = await _parametros.GetValor2Async(ParametrosAlias.Gemini, cancellationToken);
                if (string.IsNullOrWhiteSpace(apiKey))
                {
                    return Response<EventoExtraidoDto>.BadResult("No se encontró configurada la API key de Gemini.", new EventoExtraidoDto());
                }

                var modelo = await _parametros.GetValor1Async(ParametrosAlias.Gemini, cancellationToken);
                if (string.IsNullOrWhiteSpace(modelo))
                {
                    modelo = ModeloDefault;
                }

                // Convertir imagen a base64
                using var memoryStream = new MemoryStream();
                if (imageStream.CanSeek) imageStream.Position = 0;
                await imageStream.CopyToAsync(memoryStream, cancellationToken);
                var imageBytes = memoryStream.ToArray();

                if (string.IsNullOrWhiteSpace(mimeType))
                {
                    mimeType = "image/jpeg";
                }

                var prompt = BuildExtractionPrompt();

                var requestBody = new
                {
                    contents = new[]
                    {
                        new
                        {
                            role = "user",
                            parts = new object[]
                            {
                                new { text = prompt },
                                new
                                {
                                    inlineData = new
                                    {
                                        mimeType = mimeType,
                                        data = Convert.ToBase64String(imageBytes)
                                    }
                                }
                            }
                        }
                    },
                    generationConfig = new
                    {
                        temperature = 0.2,
                        responseMimeType = "application/json"
                    }
                };

                var url = $"https://generativelanguage.googleapis.com/v1beta/models/{Uri.EscapeDataString(modelo)}:generateContent?key={Uri.EscapeDataString(apiKey)}";
                var jsonRequest = JsonSerializer.Serialize(requestBody, JsonOptions);
                using var httpContent = new StringContent(jsonRequest, Encoding.UTF8, "application/json");

                using var response = await _httpClient.PostAsync(url, httpContent, cancellationToken);
                var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("Gemini rechazó el análisis de imagen del evento. Status={StatusCode}, Body={Body}", (int)response.StatusCode, responseBody);
                    return Response<EventoExtraidoDto>.BadResult($"Error al comunicarse con Gemini ({(int)response.StatusCode}).", new EventoExtraidoDto());
                }

                var jsonText = ExtraerTextoRespuesta(responseBody);
                if (string.IsNullOrWhiteSpace(jsonText))
                {
                    return Response<EventoExtraidoDto>.BadResult("Gemini no devolvió una respuesta estructurada.", new EventoExtraidoDto());
                }

                // Limpiar posibles bloques ```json ... ``` si vienen en texto
                jsonText = LimpiarBloquesJson(jsonText);

                var eventoParsed = JsonSerializer.Deserialize<EventoGeminiJson>(jsonText, JsonOptions);
                if (eventoParsed == null)
                {
                    return Response<EventoExtraidoDto>.BadResult("No se pudo interpretar el formato JSON devuelto por Gemini.", new EventoExtraidoDto());
                }

                // Mapear y normalizar fechas
                var resultado = new EventoExtraidoDto
                {
                    vchNombre = eventoParsed.Nombre?.Trim() ?? string.Empty,
                    nvchDescripcion = eventoParsed.Descripcion?.Trim() ?? string.Empty,
                    vchLugar = eventoParsed.Lugar?.Trim() ?? string.Empty,
                    vchDireccion = eventoParsed.Direccion?.Trim() ?? string.Empty,
                    vchCostoTexto = eventoParsed.CostoTexto?.Trim() ?? string.Empty,
                    vchOrganizador = eventoParsed.Organizador?.Trim() ?? string.Empty,
                    vchTelefono = eventoParsed.Telefono?.Trim() ?? string.Empty,
                    vchCorreo = eventoParsed.Correo?.Trim() ?? string.Empty,
                    vchUrlOficial = eventoParsed.UrlOficial?.Trim() ?? string.Empty,
                    vchImagenPortada = imageUrl,
                    flLatitud = eventoParsed.Latitud ?? 0,
                    flLongitud = eventoParsed.Longitud ?? 0
                };

                // Procesar fecha y hora
                if (DateTime.TryParse(eventoParsed.FechaInicioIso, out var dtInicio))
                {
                    resultado.dtFechaInicio = dtInicio;
                }
                else
                {
                    resultado.dtFechaInicio = DateTime.Now;
                }

                if (!string.IsNullOrWhiteSpace(eventoParsed.FechaFinIso) && DateTime.TryParse(eventoParsed.FechaFinIso, out var dtFin))
                {
                    resultado.dtFechaFin = dtFin;
                }

                return new Response<EventoExtraidoDto>
                {
                    IsSuccess = true,
                    Data = resultado,
                    Message = "Información del evento extraída con éxito."
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al extraer evento desde imagen con Gemini.");
                return Response<EventoExtraidoDto>.BadResult($"Error al procesar la imagen: {ex.Message}", new EventoExtraidoDto());
            }
        }

        private static string BuildExtractionPrompt()
        {
            var anioActual = DateTime.Now.Year;
            return $$"""
                Eres un asistente de inteligencia artificial especializado en extraer información de volantes, carteles y flyers promocionales de eventos en Santiago Papasquiaro, Durango, México.

                Analiza minuciosamente la imagen proporcionada y extrae los datos del evento.
                Año de referencia por defecto: {{anioActual}} (si la imagen no especifica el año, asume este año o el próximo más próximo si el mes ya pasó).

                IMPORTANTE: Devuelve EXCLUSIVAMENTE un objeto JSON válido con la siguiente estructura:
                {
                  "nombre": "Nombre principal y oficial del evento",
                  "descripcion": "Descripción redactada siguiendo las instrucciones de estilo abajo",
                  "fechaInicioIso": "YYYY-MM-DDTHH:mm:ss (Fecha y hora de inicio en formato ISO 8601)",
                  "fechaFinIso": "YYYY-MM-DDTHH:mm:ss o null si no se especifica",
                  "lugar": "Nombre del recinto, salón, auditorio o plaza donde se realiza",
                  "direccion": "Dirección o referencia del lugar (incluyendo Santiago Papasquiaro si aplica)",
                  "costoTexto": "Resumen conciso de precios (ej: 'Gratis', '$100', 'Preventa $80 / Taquilla $100', etc.)",
                  "organizador": "Empresa, asociación o persona organizadora",
                  "telefono": "Número de teléfono o WhatsApp de contacto o boletos",
                  "correo": "Correo electrónico si aparece, o null",
                  "urlOficial": "Página web, red social o link si aparece, o null",
                  "latitud": null,
                  "longitud": null
                }

                REGLAS ESPECÍFICAS PARA EL CAMPO "descripcion":
                Debes redactar una descripción atractiva, entusiasta y muy bien formateada en español de México, utilizando emojis llamativos.
                Sigue ESTRICTAMENTE esta estructura:
                1. Encabezado llamativo con emojis alusivos al tipo de evento (ejemplo: 🤼‍♂️💥 ¡La adrenalina del pancracio llega a Santiago Papasquiaro! 🎭🔥 ó 🌸 ¡EMPRENDEDORAS, ESTA INVITACIÓN ES PARA USTEDES! 🌸).
                2. Un párrafo introductorio o dos párrafos breves que inviten con entusiasmo a asistir o participar, mencionando de qué trata y quién invita.
                3. Una sección estructurada con viñetas usando exactamente estos emojis cuando la información esté presente:
                   🗓️ Fecha: [Día de la semana y fecha clara]
                   ⏰ Hora: [Hora de inicio y si hay apertura/stands]
                   📍 Lugar: [Lugar del evento]
                   🎟️ Precios / Preventa: [Detalle de boletos o precios por zona si los hay]
                   📍 Puntos de venta: [Lugares donde comprar boletos si se mencionan]
                   📲 Boletos / Registro / Información por WhatsApp: [Teléfono o contacto]
                4. Cierre motivacional o llamado a la acción.
                """;
        }

        private static string? ExtraerTextoRespuesta(string body)
        {
            using var doc = JsonDocument.Parse(body);
            var root = doc.RootElement;
            if (root.TryGetProperty("candidates", out var candidates) && candidates.GetArrayLength() > 0)
            {
                var candidate = candidates[0];
                if (candidate.TryGetProperty("content", out var content) &&
                    content.TryGetProperty("parts", out var parts) && parts.GetArrayLength() > 0)
                {
                    return parts[0].GetProperty("text").GetString();
                }
            }
            return null;
        }

        private static string LimpiarBloquesJson(string text)
        {
            text = text.Trim();
            if (text.StartsWith("```json", StringComparison.OrdinalIgnoreCase))
            {
                text = text.Substring(7);
            }
            else if (text.StartsWith("```"))
            {
                text = text.Substring(3);
            }

            if (text.EndsWith("```"))
            {
                text = text.Substring(0, text.Length - 3);
            }

            return text.Trim();
        }

        private class EventoGeminiJson
        {
            public string? Nombre { get; set; }
            public string? Descripcion { get; set; }
            public string? FechaInicioIso { get; set; }
            public string? FechaFinIso { get; set; }
            public string? Lugar { get; set; }
            public string? Direccion { get; set; }
            public string? CostoTexto { get; set; }
            public string? Organizador { get; set; }
            public string? Telefono { get; set; }
            public string? Correo { get; set; }
            public string? UrlOficial { get; set; }
            public double? Latitud { get; set; }
            public double? Longitud { get; set; }
        }
    }
}
