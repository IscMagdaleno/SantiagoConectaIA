using EngramaCoreStandar.Results;

using SantiagoConectaIA.Share.Objects.TramitesModule;
using SantiagoConectaIA.Share.PostModels.TramitesModule;

namespace SantiagoConectaIA.API.EngramaLevels.Domain.Interfaces
{
	public interface ITramiteDominio
	{
		Task<Response<IEnumerable<Requisitos>>> GetRequisitosPorTramite(PostGetRequisitosPorTramite PostModel);
		Task<Response<IEnumerable<Tramite>>> GetTramites(PostGetTramites PostModel);
		Task<Response<Requisitos>> SaveRequisito(PostSaveRequisito PostModel);
		Task<Response<Tramite>> SaveTramite(PostSaveTramite PostModel);
		Task<Response<IEnumerable<Tramite>>> SearchTramites(PostSearchTramites PostModel);
		Task<Response<IEnumerable<Tramite>>> GetTramitesCard(PostGetTramites daoModel);
		Task<Response<Tramite>> GetTramiteDetalle(PostGetTramiteDetalle postModel);
		Task<Response<Documento>> SaveDocumento(PostSaveDocumento PostModel);
	}
}
