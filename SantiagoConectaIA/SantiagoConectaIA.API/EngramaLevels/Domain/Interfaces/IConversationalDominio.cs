using EngramaCoreStandar.Results;

using SantiagoConectaIA.Share.Objects.ConversationalModule;
using SantiagoConectaIA.Share.Objects.OficinasModule;
using SantiagoConectaIA.Share.Objects.TramitesModule;
using SantiagoConectaIA.Share.PostModels.ConversationalModule;

namespace SantiagoConectaIA.API.EngramaLevels.Domain.Interfaces
{
	public interface IConversationalDominio
	{
		Task<Response<IEnumerable<TramiteCosto>>> SearchCostoForChat(PostGetByIdForChat postModel);
		Task<Response<IEnumerable<Oficina>>> SearchOficinaForChat(PostSearchForChat postModel);
		Task<Response<IEnumerable<Oficina>>> SearchOficinasByTramite(PostGetByIdForChat postModel);
		Task<Response<IEnumerable<Requisitos>>> SearchRequisitosForChat(PostGetByIdForChat postModel);
		Task<Response<IEnumerable<Tramite>>> SearchTramitesForChat(PostSearchForChat postModel);
	}
}
