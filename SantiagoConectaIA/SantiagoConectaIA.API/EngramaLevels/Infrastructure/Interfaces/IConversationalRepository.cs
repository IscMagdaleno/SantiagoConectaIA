using SantiagoConectaIA.API.EngramaLevels.Infrastructure.Entity.ConversationalModule;

namespace SantiagoConectaIA.API.EngramaLevels.Infrastructure.Interfaces
{
	public interface IConversationalRepository
	{
		Task<IEnumerable<spSearchTramitesForChat.Result>> spSearchTramitesForChat(spSearchTramitesForChat.Request req);
		Task<IEnumerable<spSearchOficinasForChat.Result>> spSearchOficinasForChat(spSearchOficinasForChat.Request req);
		Task<IEnumerable<spSearchRequisitosForChat.Result>> spSearchRequisitosForChat(spSearchRequisitosForChat.Request req);
		Task<IEnumerable<spSearchCostoForChat.Result>> spSearchCostoForChat(spSearchCostoForChat.Request req);
		Task<IEnumerable<spSearchOficinasByTramite.Result>> spSearchOficinasByTramite(spSearchOficinasByTramite.Request req);
		Task<IEnumerable<spGetChat.Result>> spGetChat(spGetChat.Request req);
		Task<spSaveChat.Result> spSaveChat(spSaveChat.Request req);
		Task<IEnumerable<spGetMensaje.Result>> spGetMensaje(spGetMensaje.Request req);
		Task<spSaveMensaje.Result> spSaveMensaje(spSaveMensaje.Request req);
	}
}
