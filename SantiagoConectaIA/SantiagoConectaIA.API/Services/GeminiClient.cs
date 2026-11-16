using System.Net;
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

		/// <summary>
		/// Intentos totales ante errores de red/TLS (0x80090330 al bajar la imagen en base64) o 429/5xx.
		/// </summary>
		private const int MaxIntentos = 3;

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
			GeminiRespuesta? ultima = null;

			for (var intento = 1; intento <= MaxIntentos; intento++)
			{
				cancellationToken.ThrowIfCancellationRequested();

				using var limite = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
				limite.CancelAfter(tiempoMaximo);

				try
				{
					using var peticion = new HttpRequestMessage(HttpMethod.Post, url)
					{
						Content = new StringContent(json, Encoding.UTF8, "application/json")
					};

					// ResponseHeadersRead: el cuerpo (la imagen en base64) se lee en streaming y no se bufferiza de golpe.
					using var respuesta = await _httpClient.SendAsync(peticion, HttpCompletionOption.ResponseHeadersRead, limite.Token);
					var cuerpo = await LeerCuerpoAsync(respuesta, limite.Token);
					var codigo = (int)respuesta.StatusCode;

					if (respuesta.IsSuccessStatusCode)
					{
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

					_logger.LogWarning("Gemini respondió {StatusCode} con el modelo {Modelo} (intento {Intento}/{Max}): {Cuerpo}",
						codigo, configuracion.Modelo, intento, MaxIntentos,
						cuerpo.Length > MaxCaracteresLogCuerpo ? cuerpo[..MaxCaracteresLogCuerpo] : cuerpo);

					ultima = new GeminiRespuesta { Exitosa = false, CodigoEstado = codigo, Cuerpo = cuerpo };
					if (!EsReintentable(codigo))
					{
						return ultima;
					}
				}
				catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
				{
					_logger.LogWarning("Gemini no respondió a tiempo con el modelo {Modelo} (intento {Intento}/{Max}).", configuracion.Modelo, intento, MaxIntentos);
					ultima = new GeminiRespuesta { Exitosa = false, CodigoEstado = (int)HttpStatusCode.RequestTimeout, Cuerpo = "timeout" };
				}
				catch (Exception ex) when (ex is HttpRequestException or IOException)
				{
					// Suele ser el fallo de Schannel al descifrar el cuerpo grande: "The decryption operation failed".
					_logger.LogWarning(ex, "Error de red/TLS al llamar a Gemini (intento {Intento}/{Max}).", intento, MaxIntentos);
					ultima = new GeminiRespuesta { Exitosa = false, CodigoEstado = (int)HttpStatusCode.BadGateway, Cuerpo = ex.Message };
				}

				if (intento < MaxIntentos)
				{
					var espera = TimeSpan.FromSeconds(Math.Pow(2, intento - 1)) + TimeSpan.FromMilliseconds(Random.Shared.Next(0, 400));
					await Task.Delay(espera, cancellationToken);
				}
			}

			return ultima ?? new GeminiRespuesta { Exitosa = false, CodigoEstado = 0, Cuerpo = string.Empty };
		}

		/// <summary>Lee el cuerpo a memoria en streaming (más estable con respuestas grandes que ReadAsStringAsync directo).</summary>
		private static async Task<string> LeerCuerpoAsync(HttpResponseMessage respuesta, CancellationToken cancellationToken)
		{
			await using var flujo = await respuesta.Content.ReadAsStreamAsync(cancellationToken);
			using var memoria = new MemoryStream();
			await flujo.CopyToAsync(memoria, cancellationToken);
			return Encoding.UTF8.GetString(memoria.GetBuffer(), 0, (int)memoria.Length);
		}

		private static bool EsReintentable(int codigo) => codigo is 408 or 429 or 500 or 502 or 503 or 504;

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
