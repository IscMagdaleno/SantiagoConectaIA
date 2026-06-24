using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.EntityFrameworkCore;
using SantiagoConectaIA.DAL.Models;

using SantiagoConectaIA.API.EngramaLevels.Domain.Interfaces;
using SantiagoConectaIA.API.SemanticKernel.Agentes;
using SantiagoConectaIA.Share.Objects.ConversationalModule;
using SantiagoConectaIA.Share.PostModels.ConversationalModule;

using System.Collections.Concurrent;

namespace SantiagoConectaIA.API.EngramaLevels.Domain.Servicios
{
	/// <summary>
	/// Implementación del servicio de orquestación que interactúa con el Agente de Ventas de Productos.
	/// Gestiona el historial de chat y lo persiste en base de datos.
	/// </summary>
	public class AgentOrchestrationService : IAgentOrchestrationService
	{
		private readonly TramitesAgentes _tramitesAgentes;
		private readonly ILogger<AgentOrchestrationService> _logger;
		private readonly IServiceScopeFactory _scopeFactory;

		public AgentOrchestrationService(
			TramitesAgentes tramitesAgentes,
			ILogger<AgentOrchestrationService> logger,
			IServiceScopeFactory scopeFactory)
		{
			_tramitesAgentes = tramitesAgentes;
			_logger = logger;
			_scopeFactory = scopeFactory;
		}

		public async Task<ChatResponseIA> ProcessUserQueryAsync(string userQuery, string userId)
		{
			_logger.LogInformation($"Orquestando consulta para usuario '{userId}': '{userQuery}'");

			using var scope = _scopeFactory.CreateScope();
			var _conversationalDominio = scope.ServiceProvider.GetRequiredService<IConversationalDominio>();

			int activeChatId = 0;
			int dbMessageCount = 0;
			var chatHistory = new ChatHistory();
			chatHistory.AddSystemMessage(_tramitesAgentes.GetSystemPrompt());

			// Buscar en BD si ya existe el chat
			var existingChatsResponse = await _conversationalDominio.GetChat(new PostGetChat { iIdProyecto = 1 });
			if (existingChatsResponse.IsSuccess && existingChatsResponse.Data != null)
			{
				var myChat = existingChatsResponse.Data.FirstOrDefault(c => c.nvchThreadId == userId);
				if (myChat != null)
				{
					activeChatId = myChat.iIdChat;
					// Cargar mensajes de la BD
					var msgsResponse = await _conversationalDominio.GetMensaje(new PostGetMensaje { iIdChat = activeChatId });
					if (msgsResponse.IsSuccess && msgsResponse.Data != null)
					{
						var sortedMsgs = msgsResponse.Data.OrderBy(x => x.iOrden).ToList();
						dbMessageCount = sortedMsgs.Count;
						foreach (var m in sortedMsgs)
						{
							if (m.nvchRol == "user") chatHistory.AddUserMessage(m.nvchContenido);
							else if (m.nvchRol == "assistant") chatHistory.AddAssistantMessage(m.nvchContenido);
						}
					}
				}
			}

			if (activeChatId == 0)
			{
				// Crear nuevo chat en BD
				var newChat = new PostSaveChat
				{
					iIdProyecto = 1, // Proyecto Santiago Conecta
					nvchNombre = "Ciudadano (" + userId.Substring(0, Math.Min(userId.Length, 8)) + ")",
					bActivo = true,
					nvchThreadId = userId,
					dtFechaCreacion = DateTime.Now
				};
				var saveChatRes = await _conversationalDominio.SaveChat(newChat);
				if (saveChatRes.IsSuccess && saveChatRes.Data != null)
				{
					activeChatId = saveChatRes.Data.iIdChat;
				}
			}

			try
			{
				// Invocar al Agente con la consulta del usuario y el historial
				string agentResponse = await _tramitesAgentes.ChatAsync(userQuery, chatHistory);

				// Guardar el mensaje del usuario y del asistente en BD
				if (activeChatId > 0)
				{
					// El orden exacto de inserción
					await _conversationalDominio.SaveMensaje(new PostSaveMensaje
					{
						iIdChat = activeChatId,
						iOrden = dbMessageCount + 1,
						nvchRol = "user",
						nvchContenido = userQuery,
						dtFecha = DateTime.Now
					});

					await _conversationalDominio.SaveMensaje(new PostSaveMensaje
					{
						iIdChat = activeChatId,
						iOrden = dbMessageCount + 2,
						nvchRol = "assistant",
						nvchContenido = agentResponse,
						dtFecha = DateTime.Now
					});
				}

				return new ChatResponseIA { nvchAgenteResponse = agentResponse };
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, $"Error al procesar la consulta del usuario '{userId}': {ex.Message}");
				return new ChatResponseIA { nvchAgenteResponse = "EXCEPCIÓN: " + ex.ToString() };
			}
		}
	}
}
