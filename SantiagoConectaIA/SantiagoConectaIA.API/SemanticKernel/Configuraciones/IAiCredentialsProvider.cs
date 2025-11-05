namespace SantiagoConectaIA.API.SemanticKernel.Configuraciones
{
	// IAiCredentialsProvider.cs
	public interface IAiCredentialsProvider
	{
		/// <summary>
		/// Obtiene la API Key para Azure OpenAI.
		/// </summary>
		/// <returns>API Key (string) o null si no configurada.</returns>
		string GetOpenAiKey();
	}
	// AiCredentialsProvider.cs

	public class AiCredentialsProvider : IAiCredentialsProvider
	{
		private readonly IConfiguration _configuration;
		private readonly ILogger<AiCredentialsProvider> _logger;

		public AiCredentialsProvider(IConfiguration configuration, ILogger<AiCredentialsProvider> logger)
		{
			_configuration = configuration;
			_logger = logger;
		}

		public string GetOpenAiKey()
		{
			// 1) Intentar configuración resuelta (KeyVault o appsettings)
			var key = _configuration["AzureOpenAI:Key"];
			if (!string.IsNullOrWhiteSpace(key))
			{
				return key;
			}

			// 2) Fallback a environment variables con dos convenciones posibles
			key = Environment.GetEnvironmentVariable("AZURE_OPENAI__KEY")
				  ?? Environment.GetEnvironmentVariable("AZURE_OPENAI_KEY");

			if (!string.IsNullOrWhiteSpace(key))
				return key;

			// 3) Si no hay clave, loguear advertencia (sin exponer la clave) y devolver null
			_logger.LogWarning("Azure OpenAI key no encontrada en IConfiguration ni en variables de entorno.");
			return null;
		}
	}

}
