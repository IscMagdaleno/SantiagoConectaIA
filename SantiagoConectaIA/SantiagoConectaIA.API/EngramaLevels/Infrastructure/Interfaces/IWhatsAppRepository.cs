using SantiagoConectaIA.API.EngramaLevels.Infrastructure.Entity.WhatsAppModule;

namespace SantiagoConectaIA.API.EngramaLevels.Infrastructure.Interfaces
{
	public interface IWhatsAppRepository
	{
		Task<spSaveWhatsAppUser.Result> spSaveWhatsAppUser(spSaveWhatsAppUser.Request request);
		Task<spSaveWhatsAppConversation.Result> spSaveWhatsAppConversation(spSaveWhatsAppConversation.Request request);
		Task<spSaveWhatsAppMessage.Result> spSaveWhatsAppMessage(spSaveWhatsAppMessage.Request request);
		Task<IEnumerable<spGetWhatsAppStats.Result>> spGetWhatsAppStats(spGetWhatsAppStats.Request request);
		Task<IEnumerable<spGetWhatsAppDailyStats.Result>> spGetWhatsAppDailyStats(spGetWhatsAppDailyStats.Request request);
	}
}
