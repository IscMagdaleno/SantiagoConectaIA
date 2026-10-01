using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using EngramaCoreStandar.Results;
using SantiagoConectaIA.Share.PostModels.PublicacionesModule;
using SantiagoConectaIA.Share.Utilities;

namespace SantiagoConectaIA.API.Services
{
    public class GeminiPublicacionService : IGeminiPublicacionService
    {
        private const string HashtagsNoticias = "#SantiagoConecta #SantiagoPapasquiaro #NoticiasSantiago";
        private const string HashtagsEmprendimientos = "#SantiagoConecta #SantiagoPapasquiaro #EmprendimientosLocales #HechoEnSantiago";
        private const string ModeloDefault = "gemini-2.5-flash";
        private const string SitioDefault = "https://www.santiagopapasquiaro.com.mx";
        private const int MaxCaracteresEntrada = 12000;

        private const string InstruccionEditor =
            "Eres el editor de redes de Santiago Conecta, el portal del municipio de Santiago Papasquiaro, Durango. " +
            "Redactas publicaciones de Facebook en español de México, claras y cercanas. " +
            "No inventas datos, nombres, precios, direcciones ni enlaces que no estén en la información recibida.";

        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };

        private readonly HttpClient _httpClient;
        private readonly IParametrosService _parametros;
        private readonly ILogger<GeminiPublicacionService> _logger;

        public GeminiPublicacionService(
            HttpClient httpClient,
            IParametrosService parametros,
            ILogger<GeminiPublicacionService> logger)
        {
            _httpClient = httpClient;
            _parametros = parametros;
            _logger = logger;
        }

        public async Task<Response<string>> MejorarAsync(PostMejorarPublicacion post, CancellationToken cancellationToken = default)
        {
            if (post == null || post.iIdNoticia <= 0 || string.IsNullOrWhiteSpace(post.vchTitulo))
            {
                return Response<string>.BadResult("Selecciona una noticia con título para mejorar el texto.", string.Empty);
            }

            var (_, noticiasUrl) = await ObtenerSitioAsync(cancellationToken);
            var enlace = $"{noticiasUrl}/{post.iIdNoticia}";
            var contenido = Recortar(TextoPublicacion.ATextoPlano(post.nvchContenido));
            var prompt = PromptNoticia(post.vchTitulo.Trim(), contenido, enlace);

            var generado = await GenerarAsync(prompt, cancellationToken);
            if (!generado.IsSuccess)
            {
                return generado;
            }

            var texto = LimpiarBloques(generado.Data);
            texto = AsegurarLinea(texto, enlace, $"👉 Conoce más y lee la nota completa en Santiago Conecta:\n{enlace}");
            texto = AsegurarLinea(texto, "#SantiagoConecta", HashtagsNoticias);
            return Ok(texto);
        }

        public async Task<Response<string>> MejorarEmprendimientoAsync(PostMejorarEmprendimiento post, CancellationToken cancellationToken = default)
        {
            if (post == null || post.iIdEmpresa <= 0 || string.IsNullOrWhiteSpace(post.nvchInformacion))
            {
                return Response<string>.BadResult("Selecciona un emprendimiento con información para mejorar el texto.", string.Empty);
            }

            var (sitio, _) = await ObtenerSitioAsync(cancellationToken);
            var enlace = $"{sitio}/emprendimientos/{post.iIdEmpresa}";
            var nombre = string.IsNullOrWhiteSpace(post.vchNombreComercial) ? "este emprendimiento" : post.vchNombreComercial.Trim();
            var prompt = PromptEmprendimiento(nombre, Recortar(post.nvchInformacion.Trim()), enlace, sitio);

            var generado = await GenerarAsync(prompt, cancellationToken);
            if (!generado.IsSuccess)
            {
                return generado;
            }

            var texto = LimpiarBloques(generado.Data);
            texto = AsegurarLinea(texto, enlace, $"👉 Conoce a {nombre} en Santiago Conecta:\n{enlace}");
            texto = AsegurarLinea(texto, "#SantiagoConecta", HashtagsEmprendimientos);
            return Ok(texto);
        }

        private async Task<Response<string>> GenerarAsync(string prompt, CancellationToken cancellationToken)
        {
            var (modelo, apiKey) = await ObtenerConfiguracionGeminiAsync(cancellationToken);
            if (string.IsNullOrWhiteSpace(apiKey))
            {
                return Response<string>.BadResult($"No está configurada la clave de Gemini (parámetro {ParametrosAlias.Gemini}).", string.Empty);
            }

            var requestBody = new GeminiGenerateRequest
            {
                SystemInstruction = new GeminiContent
                {
                    Parts = new List<GeminiPart> { new() { Text = InstruccionEditor } }
                },
                Contents = new List<GeminiContent>
                {
                    new()
                    {
                        Role = "user",
                        Parts = new List<GeminiPart> { new() { Text = prompt } }
                    }
                },
                GenerationConfig = new GeminiGenerationConfig { Temperature = 0.7 }
            };

            try
            {
                var url = $"https://generativelanguage.googleapis.com/v1beta/models/{Uri.EscapeDataString(modelo)}:generateContent?key={Uri.EscapeDataString(apiKey)}";
                var json = JsonSerializer.Serialize(requestBody, JsonOptions);
                using var content = new StringContent(json, Encoding.UTF8, "application/json");
                using var response = await _httpClient.PostAsync(url, content, cancellationToken);
                var body = await response.Content.ReadAsStringAsync(cancellationToken);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("Gemini rechazó la redacción del post. Status={StatusCode}", (int)response.StatusCode);
                    return Response<string>.BadResult("Gemini no pudo redactar la publicación.", string.Empty);
                }

                var generado = ExtraerTexto(body);
                if (string.IsNullOrWhiteSpace(generado))
                {
                    return Response<string>.BadResult("Gemini no devolvió texto para la publicación.", string.Empty);
                }

                return Ok(generado.Trim());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al consultar Gemini para la publicación.");
                return Response<string>.BadResult("No se pudo consultar Gemini.", string.Empty);
            }
        }

        private async Task<(string Modelo, string? ApiKey)> ObtenerConfiguracionGeminiAsync(CancellationToken cancellationToken)
        {
            var modelo = await _parametros.GetValor1Async(ParametrosAlias.Gemini, cancellationToken) ?? ModeloDefault;
            var apiKey = await _parametros.GetValor2Async(ParametrosAlias.Gemini, cancellationToken);

            if (modelo.StartsWith("models/", StringComparison.OrdinalIgnoreCase))
            {
                modelo = modelo["models/".Length..];
            }

            return (modelo, apiKey);
        }

        private async Task<(string Sitio, string Noticias)> ObtenerSitioAsync(CancellationToken cancellationToken)
        {
            var sitio = (await _parametros.GetValor1Async(ParametrosAlias.PublicacionesSitio, cancellationToken) ?? SitioDefault).TrimEnd('/');
            var noticias = await _parametros.GetValor2Async(ParametrosAlias.PublicacionesSitio, cancellationToken);
            return (sitio, noticias?.TrimEnd('/') ?? $"{sitio}/noticias");
        }

        private static string PromptNoticia(string titulo, string contenido, string enlace)
        {
            var cuerpo = string.IsNullOrWhiteSpace(contenido)
                ? "(La noticia no tiene cuerpo. Redacta solo con el título, sin inventar detalles.)"
                : contenido;

            return
                $"""
                Redacta UNA publicación de Facebook para esta noticia. Devuelve solo el texto listo para copiar, sin título de sección, sin comillas y sin bloques de código.

                Estructura obligatoria:
                1. Primera línea: empieza exactamente con "Santiago Conecta presenta esta noticia:" y después un título corto y llamativo, con 1 o 2 emojis relacionados. El título debe destacar lo principal de la nota.
                2. Un resumen breve, de 2 a 4 párrafos cortos, en tono cercano del municipio. Deja claro que la información está publicada en Santiago Conecta y que ahí se lee completa.
                3. Si la nota menciona personas o instituciones, nómbralas solo con los datos que aparezcan. No agregues cargos ni nombres que no estén en el texto.
                4. Cierre invitando a leer la nota completa y a comentar. Incluye esta línea tal cual:
                👉 Conoce más y lee la nota completa en Santiago Conecta:
                {enlace}
                5. Última línea, exactamente estos hashtags:
                {HashtagsNoticias}

                Título: {titulo}

                Contenido:
                {cuerpo}
                """;
        }

        private static string PromptEmprendimiento(string nombre, string informacion, string enlace, string sitio)
        {
            return
                $"""
                Redacta UNA publicación de Facebook para presentar este emprendimiento local. Devuelve solo el texto listo para copiar, sin comillas, sin bloques de código y sin formato Markdown (nada de asteriscos ni almohadillas para títulos).

                Estructura obligatoria, en este orden:
                1. Primera línea: una frase llamativa relacionada con el giro del negocio, con 1 o 2 emojis.
                2. Un párrafo que empiece con "En Santiago Conecta nos encanta apoyar a los talentos locales" y presente a {nombre}: qué hace, qué lo distingue y, si existen, una idea breve de su misión o visión. Usa 1 o 2 emojis.
                3. Una línea con 📍 que invite a visitarlo y, debajo, la dirección o direcciones tal como aparecen. Omite esta sección si no hay dirección.
                4. Una sección "Productos y Servicios Destacados:" con un renglón por producto o servicio: nombre, descripción corta y precio si existe (formato $123 MXN; si hay precio con descuento, menciónalo). Máximo 10 renglones; si hay más, elige los más representativos.
                5. Una sección "📲 Contáctalo:" con WhatsApp, teléfono, Facebook, Instagram u otras redes, copiando los enlaces exactamente como vienen. Omite los que no existan.
                6. Una invitación a cotizar o pedir sin compromiso, seguida de esta línea tal cual:
                👉 Conoce a {nombre} en Santiago Conecta:
                {enlace}
                7. Cierre: "¿Tienes un negocio y quieres formar parte de Santiago Conecta? Regístrate en {sitio} y haz que más gente conozca tus productos y servicios."
                8. Última línea, exactamente estos hashtags:
                {HashtagsEmprendimientos}

                Información del emprendimiento:
                {informacion}
                """;
        }

        private static string LimpiarBloques(string texto)
        {
            texto = Regex.Replace(texto, "^```[a-zA-Z]*\\s*", string.Empty);
            return Regex.Replace(texto, "\\s*```$", string.Empty).Trim();
        }

        private static string AsegurarLinea(string texto, string marcador, string linea)
        {
            return texto.Contains(marcador, StringComparison.OrdinalIgnoreCase)
                ? texto
                : $"{texto}\n\n{linea}".Trim();
        }

        private static string Recortar(string texto)
        {
            return texto.Length > MaxCaracteresEntrada ? texto[..MaxCaracteresEntrada] : texto;
        }

        private static Response<string> Ok(string texto)
        {
            return new Response<string>
            {
                IsSuccess = true,
                Data = texto,
                Message = "Texto listo para revisar."
            };
        }

        private static string? ExtraerTexto(string body)
        {
            var parsed = JsonSerializer.Deserialize<GeminiGenerateResponse>(body, JsonOptions);
            return parsed?.Candidates?
                .SelectMany(c => c.Content?.Parts ?? new List<GeminiPart>())
                .Select(p => p.Text)
                .FirstOrDefault(t => !string.IsNullOrWhiteSpace(t));
        }

        private sealed class GeminiGenerateRequest
        {
            public GeminiContent? SystemInstruction { get; set; }
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
            public string Text { get; set; } = string.Empty;
        }

        private sealed class GeminiGenerationConfig
        {
            public double Temperature { get; set; }
        }

        private sealed class GeminiGenerateResponse
        {
            public List<GeminiCandidate>? Candidates { get; set; }
        }

        private sealed class GeminiCandidate
        {
            public GeminiContent? Content { get; set; }
        }
    }
}
