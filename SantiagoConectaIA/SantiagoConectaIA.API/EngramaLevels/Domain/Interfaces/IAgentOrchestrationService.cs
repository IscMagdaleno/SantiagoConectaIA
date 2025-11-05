using SantiagoConectaIA.Share.Objects.ConversationalModule;

namespace SantiagoConectaIA.API.EngramaLevels.Domain.Interfaces
{
	public interface IAgentOrchestrationService
	{
		Task<ChatResponseIA> ProcessUserQueryAsync(string userQuery, string userId);
	}
}
