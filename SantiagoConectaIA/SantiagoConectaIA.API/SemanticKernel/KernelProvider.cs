using Microsoft.SemanticKernel;

using SantiagoConectaIA.API.EngramaLevels.Domain.Interfaces;
using SantiagoConectaIA.API.SemanticKernel.Plugins;

namespace SantiagoConectaIA.API.SemanticKernel
{
	public class KernelProvider
	{
		private readonly IConfiguration _configuration;
		private readonly ILogger<KernelProvider> _logger;
		private IConversationalDominio _conversationalDominio;
		private Kernel _kernelInstance;

		public KernelProvider(
			IConfiguration configuration,
			ILogger<KernelProvider> logger,
			IConversationalDominio conversationalDominio)
		{
			_configuration = configuration;
			_logger = logger;
			_conversationalDominio = conversationalDominio;
		}

		/// <summary>
		/// Devuelve una instancia singleton del Kernel, inicializándola la primera vez.
		/// </summary>
		public Kernel GetKernel()
		{
			if (_kernelInstance != null) return _kernelInstance;

			// *************** 1. OBTENER CONFIGURACIÓN DE GEMINI ***************
			// Usamos la misma sección del appsettings.json, pero leemos la clave
			var modelName = _configuration["Gemini:ModelName"] ?? "gemini-2.5-flash"; // Modelo de Gemini a usar
			var apiKey = _configuration["Gemini:KeyVaultSecretName"]; // La clave que apunta a tu API Key

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

			_kernelInstance.Plugins.AddFromObject(new ConsultaPlugin(_conversationalDominio), "ConsultaBaseDatos");
			_logger.LogInformation("ConsultaPlugin registrado exitosamente con el Kernel.");


			return _kernelInstance;
		}
	}
}