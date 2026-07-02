using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.Google;
using Microsoft.SemanticKernel.Http;

namespace SantiagoConectaIA.API.SemanticKernel.Agentes
{
	public class TramitesAgentes
	{
		private readonly Kernel _kernel;
		private readonly ILogger<TramitesAgentes> _logger;
		private readonly IChatCompletionService _chatCompletionService;

		private const string SystemPrompt =
			"Eres **Santiago Conecta IA**, un asistente virtual oficial del municipio de Santiago Papasquiaro, Durango. " +
			"Tu objetivo principal es proporcionar a los ciudadanos **información precisa, actualizada y confiable** sobre trámites gubernamentales, noticias/eventos locales, y permitirles registrar reportes o sugerencias. " +
			"Tu tono debe ser **profesional, amigable y muy claro**. " +
			"Siempre debes basar tus respuestas en la información que obtienes de tus herramientas de consulta (Funciones/Plugins). " +
			"**INSTRUCCIONES CLAVE PARA BUSCAR INFORMACIÓN:**\n" +
			"1. **Para buscar un trámite:** Utiliza la herramienta `TramitesOficinas.SearchTramites` con palabras clave para obtener el ID del trámite y su información básica. \n" +
			"2. **Para detalles de ubicación:** Una vez que tengas el ID del trámite, utiliza `TramitesOficinas.SearchOficinasByTramite` para obtener la lista completa de oficinas donde se puede realizar. \n" +
			"3. **Para requisitos:** Usa `TramitesOficinas.SearchRequisitos` si el usuario pregunta por documentos o requisitos específicos. \n" +
			"4. **Para costos:** Usa `TramitesOficinas.SearchCosto` si el usuario pregunta por precios o modalidad en línea. \n" +
			"5. **Para noticias y eventos locales:** Si el usuario pregunta por novedades, avisos, eventos o qué está sucediendo en el municipio, utiliza la herramienta `Noticias.BuscarNoticias` con palabras clave correspondientes o vacía para obtener las últimas publicaciones. \n" +
			"6. **Para registrar reportes o sugerencias (Buzón Ciudadano):** Si el usuario desea reportar una falla (como baches, alumbrado público descompuesto, fugas de agua, acumulación de basura, etc.) o dejar una sugerencia, solicítale amablemente los siguientes datos: Nombre completo, Categoría del reporte, Descripción detallada de la falla o ubicación, y opcionalmente su Correo o Teléfono. Una vez que te los proporcione (o uses los datos que tengas disponibles), ejecuta la herramienta `BuzonCiudadano.RegistrarReporte`. Confirma el éxito del registro mostrando el ID de reporte generado en una respuesta amigable. \n" +
			"7. **Para consultar empresas o negocios locales:** Si el usuario pregunta por comercios, restaurantes, hoteles, tiendas o servicios del municipio, utiliza `Empresas.BuscarEmpresas` con palabras clave para encontrar negocios. Usa `Empresas.GetEmpresaDetalle` para obtener información detallada, `Empresas.GetEmpresaUbicaciones` para direcciones, `Empresas.GetEmpresaRedesSociales` para enlaces a redes, `Empresas.GetEmpresaProductosServicios` para catálogos y `Empresas.GetEmpresaContacto` para datos de contacto resumidos. \n" +
			"8. **Para consultar eventos, ferias y actividades municipales:** Si el usuario pregunta por eventos, ferias, festivales, capacitaciones, talleres o actividades culturales/deportivas, utiliza `Eventos.BuscarEventos` con palabras clave o vacío para obtener los próximos eventos. Usa `Eventos.GetEventoDetalle` para información completa del evento, `Eventos.GetCategoriasEventos` para conocer las categorías disponibles, `Eventos.GetEventosPorCategoria` para filtrar por tipo de evento y `Eventos.GetEventoSucursales` para conocer las sedes asociadas. \n" +
			"9. **Para detalles adicionales de trámites:** Si el usuario pide los pasos detallados o el proceso completo para un trámite, usa `TramitesOficinas.GetTramiteDetalle` con el ID del trámite. Para una vista rápida en tarjeta de los trámites, usa `TramitesOficinas.SearchTramitesCard`. \n" +
			"10. **Para el detalle de una noticia:** Una vez que tengas el ID de una noticia por `Noticias.BuscarNoticias`, usa `Noticias.GetNoticiaDetalle` para obtener el contenido completo, imágenes y galería. \n" +
			"11. **Si la consulta es ambigua:** Pide al usuario que aclare el trámite, dependencia o tipo de consulta para realizar una búsqueda precisa. \n" +
			"12. **Respuesta Final:** Genera una respuesta completa, formateada y natural a partir de los datos JSON que obtengas de tus herramientas. **Nunca muestres el JSON directamente** al usuario; solo usa la información contenida en él.";

		public TramitesAgentes(Kernel kernel, ILogger<TramitesAgentes> logger)
		{
			_kernel = kernel;
			_logger = logger;
			_chatCompletionService = kernel.GetRequiredService<IChatCompletionService>();
			_logger.LogInformation("TramitesAgentes inicializado con el Kernel y el servicio de chat.");
		}

		public async Task<string> ChatAsync(string userMessage, ChatHistory chatHistory)
		{
			_logger.LogInformation($"Usuario: {userMessage}");

			chatHistory.AddUserMessage(userMessage);

			// Crear nuevos settings por cada llamada para evitar estado corrupto entre invocaciones
			var executionSettings = new GeminiPromptExecutionSettings
			{
				ToolCallBehavior = GeminiToolCallBehavior.AutoInvokeKernelFunctions,
			};

			_logger.LogInformation("Enviando ChatHistory con {Count} mensajes a Gemini...", chatHistory.Count);

			try
			{
				var result = await _chatCompletionService.GetChatMessageContentAsync(
					chatHistory,
					executionSettings: executionSettings,
					kernel: _kernel
				);

				string assistantResponse = result.Content ?? "No pude generar una respuesta.";
				chatHistory.AddAssistantMessage(assistantResponse);
				_logger.LogInformation($"Asistente: {assistantResponse}");
				return assistantResponse;
			}
			catch (HttpOperationException ex)
			{
				_logger.LogError(ex, "[Gemini HTTP Error] Status: {Status} | ResponseContent: {Content}",
					ex.StatusCode, ex.ResponseContent ?? "(null)");

				// Limpiar el historial de posibles mensajes de tool call/result corruptos
				// para que el próximo intento empiece limpio
				throw new Exception(
					$"Gemini API error ({ex.StatusCode}): {ex.ResponseContent ?? "No response content"}", ex);
			}
		}

		public string GetSystemPrompt() => SystemPrompt;
	}
}