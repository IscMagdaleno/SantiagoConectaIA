using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SantiagoConectaIA.API.Services
{
	public class GeminiClient : IGeminiClient
	{
		private const string UrlBase = "https://generativelanguage.googleapis.com/v1beta/models";
		private const string PrefijoModelo = "models/";
		private const int MaxCaracteresLogCuerpo = 500;

		private static readonly JsonSerializerOptions JsonOptions = new()
		{
			PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
			PropertyNameCaseInsensitive = true,
			DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
		};

		private readonly HttpClient _httpClient;
		private readonly IParametrosService _parametros;
		private readonly ILogger<GeminiClient> _logger;

		public GeminiClient(HttpClient httpClient, IParametrosService parametros, ILogger<GeminiClient> logger)
		{
			_httpClient = httpClient;
			_parametros = parametros;
			_logger = logger;
		}

		public async Task<GeminiConfiguracion> ObtenerConfiguracionAsync(string aliasModelo, string modeloPorDefecto, CancellationToken cancellationToken = default)
		{
			var modelo = await _parametros.GetValor1Async(aliasModelo, cancellationToken) ?? modeloPorDefecto;
			if (modelo.StartsWith(PrefijoModelo, StringComparison.OrdinalIgnoreCase))
			{
				modelo = modelo[PrefijoModelo.Length..];
			}

			var apiKey = await _parametros.GetValor2Async(ParametrosAlias.Gemini, cancellationToken);
			return new GeminiConfiguracion(modelo, apiKey);
		}

		public async Task<GeminiRespuesta> GenerarContenidoAsync(GeminiConfiguracion configuracion, GeminiSolicitud solicitud, TimeSpan tiempoMaximo, CancellationToken cancellationToken = default)
		{
			var url = $"{UrlBase}/{Uri.EscapeDataString(configuracion.Modelo)}:generateContent?key={Uri.EscapeDataString(configuracion.ApiKey!)}";
			var json = JsonSerializer.Serialize(solicitud, JsonOptions);
			using var contenido = new StringContent(json, Encoding.UTF8, "application/json");

			using var limite = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
			limite.CancelAfter(tiempoMaximo);

			using var respuesta = await _httpClient.PostAsync(url, contenido, limite.Token);
			var cuerpo = await respuesta.Content.ReadAsStringAsync(limite.Token);
			var codigo = (int)respuesta.StatusCode;

			if (!respuesta.IsSuccessStatusCode)
			{
				_logger.LogWarning("Gemini respondió {StatusCode} con el modelo {Modelo}: {Cuerpo}",
					codigo, configuracion.Modelo, cuerpo.Length > MaxCaracteresLogCuerpo ? cuerpo[..MaxCaracteresLogCuerpo] : cuerpo);
				return new GeminiRespuesta { Exitosa = false, CodigoEstado = codigo, Cuerpo = cuerpo };
			}

			var parseada = JsonSerializer.Deserialize<GeminiRespuestaJson>(cuerpo, JsonOptions);
			return new GeminiRespuesta
			{
				Exitosa = true,
				CodigoEstado = codigo,
				Cuerpo = cuerpo,
				Partes = parseada?.Candidates?.SelectMany(c => c.Content?.Parts ?? new List<GeminiPart>()).ToList() ?? new List<GeminiPart>(),
				MotivoFin = parseada?.Candidates?.Select(c => c.FinishReason).FirstOrDefault(r => !string.IsNullOrWhiteSpace(r))
			};
		}

		private sealed class GeminiRespuestaJson
		{
			public List<Candidato>? Candidates { get; set; }
		}

		private sealed class Candidato
		{
			public GeminiContent? Content { get; set; }
			public string? FinishReason { get; set; }
		}
	}
}
