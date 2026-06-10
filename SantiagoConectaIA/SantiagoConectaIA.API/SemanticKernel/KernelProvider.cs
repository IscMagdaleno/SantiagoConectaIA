using Microsoft.SemanticKernel;

using SantiagoConectaIA.API.EngramaLevels.Domain.Interfaces;
using SantiagoConectaIA.API.SemanticKernel.Plugins;
using SantiagoConectaIA.DAL.Provider;

namespace SantiagoConectaIA.API.SemanticKernel
{
	public class KernelProvider
	{
		private readonly IConfiguration _configuration;
		private readonly ILogger<KernelProvider> _logger;
		private readonly IServiceScopeFactory _scopeFactory;
		private Kernel _kernelInstance;

		public KernelProvider(
			IConfiguration configuration,
			ILogger<KernelProvider> logger,
			IServiceScopeFactory scopeFactory)
		{
			_configuration = configuration;
			_logger = logger;
			_scopeFactory = scopeFactory;
		}

		/// <summary>
		/// Devuelve una instancia singleton del Kernel, inicializándola la primera vez.
		/// </summary>
		public Kernel GetKernel()
		{
			if (_kernelInstance != null) return _kernelInstance;

			// *************** 1. OBTENER CONFIGURACIÓN DE GEMINI ***************
			// Se consulta desde la base de datos usando el alias "key.gemini"
			string modelName = "gemini-2.5-flash";
			string apiKey = null;

			using (var scope = _scopeFactory.CreateScope())
			{
				var catalogosProvider = scope.ServiceProvider.GetRequiredService<ICatalogosProvider>();
				var parametro = catalogosProvider.GetParametroByAliasAsync("key.gemini").GetAwaiter().GetResult();

				if (parametro != null)
				{
					if (!string.IsNullOrWhiteSpace(parametro.NvchValor1)) modelName = parametro.NvchValor1;
					if (!string.IsNullOrWhiteSpace(parametro.NvchValor2)) apiKey = parametro.NvchValor2;
				}
			}

			if (string.IsNullOrWhiteSpace(apiKey))
			{
				_logger.LogError("Configuración del LLM incompleta. La clave de API de Gemini es requerida.");
				throw new InvalidOperationException("Falta configuración de la API Key de Gemini.");
			}

			_logger.LogInformation("Inicializando Kernel Semantic Kernel con Google Gemini model='{modelName}'", modelName);

			var kernelBuilder = Kernel.CreateBuilder();

			// *************** 2. AGREGAR EL CONECTOR DE GEMINI ***************
			// El conector de Google Gemini usa la API Key directamente.
			kernelBuilder.AddGoogleAIGeminiChatCompletion(
				modelId: modelName,
				apiKey: apiKey // Pasa la API Key directamente
			);

			// Build kernel
			_kernelInstance = kernelBuilder.Build();

			_kernelInstance.Plugins.AddFromObject(new ConsultaPlugin(_scopeFactory), "TramitesOficinas");
			_logger.LogInformation("ConsultaPlugin registrado exitosamente con el Kernel como 'TramitesOficinas'.");

			_kernelInstance.Plugins.AddFromObject(new NoticiasPlugin(_scopeFactory), "Noticias");
			_logger.LogInformation("NoticiasPlugin registrado exitosamente con el Kernel como 'Noticias'.");

			_kernelInstance.Plugins.AddFromObject(new BuzonCiudadanoPlugin(_scopeFactory), "BuzonCiudadano");
			_logger.LogInformation("BuzonCiudadanoPlugin registrado exitosamente con el Kernel como 'BuzonCiudadano'.");

			return _kernelInstance;
		}
	}
}