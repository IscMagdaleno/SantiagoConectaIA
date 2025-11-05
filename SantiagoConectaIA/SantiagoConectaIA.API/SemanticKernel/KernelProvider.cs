using Microsoft.SemanticKernel;

using OpenAI; // OpenAI .NET client (si usas OpenAI SDK)

using SantiagoConectaIA.API.EngramaLevels.Domain.Interfaces;
using SantiagoConectaIA.API.SemanticKernel.Configuraciones;
using SantiagoConectaIA.API.SemanticKernel.Plugins;

using System.ClientModel;

namespace SantiagoConectaIA.API.SemanticKernel
{
	public class KernelProvider
	{
		private readonly IConfiguration _configuration;
		private readonly ILogger<KernelProvider> _logger;
		private readonly IAiCredentialsProvider _aiCredentials;
		private readonly ILoggerFactory _loggerFactory;
		private IConversationalDominio _conversationalDominio;
		private Kernel _kernelInstance;

		public KernelProvider(
			IConfiguration configuration,
			ILogger<KernelProvider> logger,
			IAiCredentialsProvider aiCredentials,
			IConversationalDominio conversationalDominio,
			ILoggerFactory loggerFactory)
		{
			_configuration = configuration;
			_logger = logger;
			_aiCredentials = aiCredentials;
			_conversationalDominio = conversationalDominio;
			_loggerFactory = loggerFactory;
		}

		/// <summary>
		/// Devuelve una instancia singleton del Kernel, inicializándola la primera vez.
		/// </summary>
		public Kernel GetKernel()
		{
			if (_kernelInstance != null) return _kernelInstance;



			var endpoint = _configuration["AzureOpenAI:Endpoint"];
			var deployment = _configuration["AzureOpenAI:Deployment"];
			var key = _configuration["AzureOpenAI:KeyVaultSecretName"];

			if (string.IsNullOrWhiteSpace(endpoint) || string.IsNullOrWhiteSpace(deployment) || string.IsNullOrWhiteSpace(key))
			{
				_logger.LogError("Configuración del LLM incompleta. AzureOpenAI:Endpoint / Deployment / Key requeridos.");
				throw new InvalidOperationException("Falta configuración de Azure OpenAI.");
			}

			_logger.LogInformation("Inicializando Kernel Semantic Kernel con Azure OpenAI deployment='{deployment}'", deployment);

			// Crear cliente OpenAI (OpenAI .NET) y conectarlo al Kernel
			var openAiClient = new OpenAIClient(
				new ApiKeyCredential(key),
				new OpenAIClientOptions { Endpoint = new Uri(endpoint) }
			);


			var kernelBuilder = Kernel.CreateBuilder();

			kernelBuilder.AddOpenAIChatCompletion(deployment, openAIClient: openAiClient);

			// Build kernel
			_kernelInstance = kernelBuilder.Build();

			_kernelInstance.Plugins.AddFromObject(new ConsultaPlugin(_conversationalDominio), "TramiesOficinas");
			_logger.LogInformation("ConsultaPlugin registrado exitosamente con el Kernel.");


			return _kernelInstance;
		}
	}

}
