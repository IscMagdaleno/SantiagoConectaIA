using System.Text.RegularExpressions;

namespace SantiagoConectaIA.API.Services
{
	/// <summary>Clave y modelo de Gemini ya resueltos desde los parámetros.</summary>
	public sealed record GeminiConfiguracion(string Modelo, string? ApiKey)
	{
		public bool Configurada => !string.IsNullOrWhiteSpace(ApiKey);
	}

	/// <summary>Cuerpo de <c>models/{modelo}:generateContent</c>.</summary>
	public sealed class GeminiSolicitud
	{
		public GeminiContent? SystemInstruction { get; set; }
		public List<GeminiContent> Contents { get; set; } = new();
		public GeminiGenerationConfig? GenerationConfig { get; set; }

		public static GeminiSolicitud DeTexto(string prompt, string? instruccionSistema, GeminiGenerationConfig config) => new()
		{
			SystemInstruction = instruccionSistema == null ? null : GeminiContent.Texto(instruccionSistema),
			Contents = { GeminiContent.Usuario(new GeminiPart { Text = prompt }) },
			GenerationConfig = config
		};
	}

	public sealed class GeminiContent
	{
		public string? Role { get; set; }
		public List<GeminiPart> Parts { get; set; } = new();

		public static GeminiContent Texto(string texto) => new() { Parts = { new GeminiPart { Text = texto } } };

		public static GeminiContent Usuario(params GeminiPart[] partes) => new() { Role = "user", Parts = partes.ToList() };
	}

	public sealed class GeminiPart
	{
		public string? Text { get; set; }
		public GeminiInlineData? InlineData { get; set; }

		public static GeminiPart Imagen(string mimeType, byte[] bytes) => new()
		{
			InlineData = new GeminiInlineData { MimeType = mimeType, Data = Convert.ToBase64String(bytes) }
		};
	}

	public sealed class GeminiInlineData
	{
		public string? MimeType { get; set; }
		public string? Data { get; set; }
	}

	public sealed class GeminiGenerationConfig
	{
		public double? Temperature { get; set; }
		public string? ResponseMimeType { get; set; }
		public List<string>? ResponseModalities { get; set; }
	}

	/// <summary>Resultado de una llamada a Gemini, ya con las partes de texto e imagen extraídas.</summary>
	public sealed class GeminiRespuesta
	{
		public bool Exitosa { get; init; }
		public int CodigoEstado { get; init; }
		public string Cuerpo { get; init; } = string.Empty;
		public IReadOnlyList<GeminiPart> Partes { get; init; } = Array.Empty<GeminiPart>();
		public string? MotivoFin { get; init; }

		public string? PrimerTexto => Partes.Select(p => p.Text).FirstOrDefault(t => !string.IsNullOrWhiteSpace(t));

		public GeminiInlineData? PrimeraImagen => Partes.FirstOrDefault(p => !string.IsNullOrWhiteSpace(p.InlineData?.Data))?.InlineData;
	}

	public static class GeminiTexto
	{
		/// <summary>Quita las cercas de código (<c>```json ... ```</c>) que a veces agrega el modelo.</summary>
		public static string QuitarBloquesDeCodigo(string texto)
		{
			texto = Regex.Replace(texto.Trim(), "^```[a-zA-Z]*\\s*", string.Empty);
			return Regex.Replace(texto, "\\s*```$", string.Empty).Trim();
		}
	}
}
