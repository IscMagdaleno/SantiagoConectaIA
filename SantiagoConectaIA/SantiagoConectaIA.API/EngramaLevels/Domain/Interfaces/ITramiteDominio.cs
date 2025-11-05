using EngramaCoreStandar.Results;

using SantiagoConectaIA.Share.Objects.TramitesModule;
using SantiagoConectaIA.Share.PostModels.TramitesModule;

namespace SantiagoConectaIA.API.EngramaLevels.Domain.Interfaces
{
	public interface ITramiteDominio
	{
		Task<Response<IEnumerable<RequisitosPorTramite>>> GetRequisitosPorTramite(PostGetRequisitosPorTramite PostModel);
		Task<Response<IEnumerable<Tramite>>> GetTramites(PostGetTramites PostModel);
		Task<Response<RequisitosPorTramite>> SaveRequisito(PostSaveRequisito PostModel);
		Task<Response<Tramite>> SaveTramite(PostSaveTramite PostModel);
		Task<Response<IEnumerable<Tramite>>> SearchTramites(PostSearchTramites PostModel);
	}
}
