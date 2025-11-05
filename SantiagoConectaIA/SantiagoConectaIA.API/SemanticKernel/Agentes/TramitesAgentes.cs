using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;

namespace SantiagoConectaIA.API.SemanticKernel.Agentes
{
	public class TramitesAgentes
	{
		private readonly Kernel _kernel;
		private readonly ILogger<TramitesAgentes> _logger;
		private readonly IChatCompletionService _chatCompletionService;
		private readonly OpenAIPromptExecutionSettings _executionSettings;

		// El prompt del sistema, que define el rol y las capacidades del asistente
		private const string SystemPrompt =
			"Eres un asistente de ventas de Siemens amigable y servicial. " +
			"Tu objetivo es ayudar a los usuarios a encontrar productos Siemens. " +
			"Puedes buscar productos en el catálogo utilizando la herramienta 'ProductCatalog.SearchProducts'. " +
			"Cuando busques productos, utiliza siempre esta herramienta. " +
			"Si la consulta del usuario es ambigua o no está clara para la búsqueda de productos, pide más detalles. " +
			"Siempre responde de manera concisa y profesional, pero con un toque amigable.";


		public TramitesAgentes(Kernel kernel, ILogger<TramitesAgentes> logger)
		{
			_kernel = kernel;
			_logger = logger;

			// Obtenemos el servicio de chat del Kernel
			_chatCompletionService = kernel.GetRequiredService<IChatCompletionService>();

			// Configuramos las opciones de ejecución para permitir la llamada automática a funciones (herramientas)
			_executionSettings = new OpenAIPromptExecutionSettings
			{
				FunctionChoiceBehavior = FunctionChoiceBehavior.Auto()
			};

			_logger.LogInformation("ProductSalesAgent inicializado con el Kernel y el servicio de chat.");
		}

		/// <summary>
		/// Inicia o continúa una conversación con el asistente.
		/// </summary>
		/// <param name="userMessage">El mensaje del usuario.</param>
		/// <param name="chatHistory">El historial de chat actual para la conversación.</param>
		/// <returns>La respuesta del asistente.</returns>
		public async Task<string> ChatAsync(string userMessage, ChatHistory chatHistory) // Modificado para aceptar ChatHistory
		{
			_logger.LogInformation($"Usuario: {userMessage}");

			// El prompt del sistema ya debe estar en chatHistory si es una nueva conversación.
			// Solo añadimos el mensaje del usuario.
			chatHistory.AddUserMessage(userMessage);

			var result = await _chatCompletionService.GetChatMessageContentAsync(
				chatHistory,
				executionSettings: _executionSettings,
				kernel: _kernel // Pasamos el kernel para que el chat completion service pueda acceder a los plugins
			);

			string assistantResponse = result.Content ?? "No pude generar una respuesta.";

			// Añadir la respuesta del asistente al historial para futuras interacciones
			chatHistory.AddAssistantMessage(assistantResponse);

			_logger.LogInformation($"Asistente: {assistantResponse}");
			return assistantResponse;
		}

		/// <summary>
		/// Obtiene el prompt del sistema configurado para el agente.
		/// </summary>
		/// <returns>El prompt del sistema.</returns>
		public string GetSystemPrompt() => SystemPrompt;
	}
}
