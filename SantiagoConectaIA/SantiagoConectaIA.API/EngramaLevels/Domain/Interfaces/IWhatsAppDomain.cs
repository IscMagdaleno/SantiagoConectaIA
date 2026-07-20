using EngramaCoreStandar.Results;

using SantiagoConectaIA.Share.Objects.WhatsAppModule;
using SantiagoConectaIA.Share.PostModels.WhatsAppModule;

namespace SantiagoConectaIA.API.EngramaLevels.Domain.Interfaces
{
	public interface IWhatsAppDomain
	{
		Task<Response<WhatsAppUser>> SaveWhatsAppUser(PostSaveWhatsAppUser postModel);
		Task<Response<WhatsAppConversation>> SaveWhatsAppConversation(PostSaveWhatsAppConversation postModel);
		Task<Response<WhatsAppMessage>> SaveWhatsAppMessage(PostSaveWhatsAppMessage postModel);
		Task<Response<WhatsAppStats>> GetWhatsAppStats();
		Task<Response<IEnumerable<WhatsAppDailyStats>>> GetWhatsAppDailyStats(PostGetWhatsAppDailyStats postModel);
	}
}
