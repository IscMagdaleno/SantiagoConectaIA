using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;

namespace SantiagoConectaIA.API.SemanticKernel.Agentes
{
	public class TramitesAgentes
	{
		private readonly Kernel _kernel;
		private readonly ILogger<TramitesAgentes> _logger;
		private readonly IChatCompletionService _chatCompletionService;

		private readonly PromptExecutionSettings _executionSettings;

		// El prompt del sistema, que define el rol y las capacidades del asistente
		private const string SystemPrompt =
			"Eres **Santiago Conecta IA**, un asistente virtual oficial del municipio de Santiago Papasquiaro, Durango. " +
			"Tu objetivo principal es proporcionar a los ciudadanos **información precisa, actualizada y confiable** sobre los trámites gubernamentales que pueden realizar. " +
			"Tu tono debe ser **profesional, amigable y muy claro**. " +
			"Siempre debes basar tus respuestas en la información que obtienes de tus herramientas de consulta (Funciones/Plugins). " +

			// Instrucciones específicas para el uso de herramientas:
			"**INSTRUCCIONES CLAVE PARA BUSCAR INFORMACIÓN:**\n" +
			"1. **Para buscar un trámite:** Utiliza la herramienta `TramitesOficinas.SearchTramites` con palabras clave para obtener el ID del trámite y su información básica. \n" +
			"2. **Para detalles de ubicación:** Una vez que tengas el ID del trámite, utiliza `TramitesOficinas.SearchOficinasByTramite` para obtener la lista completa de oficinas donde se puede realizar. \n" +
			"3. **Para requisitos:** Usa `TramitesOficinas.SearchRequisitos` si el usuario pregunta por documentos o requisitos específicos. \n" +
			"4. **Para costos:** Usa `TramitesOficinas.SearchCosto` si el usuario pregunta por precios o modalidad en línea. \n" +
			"5. **Si la consulta es ambigua:** Pide al usuario que aclare el nombre del trámite o la dependencia para realizar una búsqueda precisa. \n" +
			"6. **Respuesta Final:** Genera una respuesta completa y natural a partir de los datos JSON que obtengas de tus herramientas. **Nunca muestres el JSON directamente** al usuario; solo usa la información contenida en él.";

		public TramitesAgentes(Kernel kernel, ILogger<TramitesAgentes> logger)
		{
			_kernel = kernel;
			_logger = logger;

			// Obtenemos el servicio de chat del Kernel
			_chatCompletionService = kernel.GetRequiredService<IChatCompletionService>();

			// CAMBIO CLAVE 2: Usar la clase base sin configuración específica de OpenAI.
			// Esto permite que el conector de Gemini maneje la llamada a funciones correctamente.
			_executionSettings = new PromptExecutionSettings();

			_logger.LogInformation("TramitesAgentes inicializado con el Kernel y el servicio de chat.");
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

			chatHistory.AddUserMessage(userMessage);

			var result = await _chatCompletionService.GetChatMessageContentAsync(
				chatHistory,
				executionSettings: _executionSettings,
				kernel: _kernel // Se pasa el kernel para que el servicio tenga acceso a los plugins registrados
			);

			string assistantResponse = result.Content ?? "No pude generar una respuesta.";

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