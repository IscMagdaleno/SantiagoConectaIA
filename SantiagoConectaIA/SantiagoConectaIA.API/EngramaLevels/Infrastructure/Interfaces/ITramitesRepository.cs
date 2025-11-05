using SantiagoConectaIA.API.EngramaLevels.Infrastructure.Entity.TramitesModule;

namespace SantiagoConectaIA.API.EngramaLevels.Infrastructure.Interfaces
{
	public interface ITramitesRepository
	{
		Task<IEnumerable<spGetTramites.Result>> spGetTramites(spGetTramites.Request request);
		Task<IEnumerable<spSearchTramites.Result>> spSearchTramites(spSearchTramites.Request request);
		Task<spSaveTramite.Result> spSaveTramite(spSaveTramite.Request request);

		Task<IEnumerable<spGetRequisitosPorTramite.Result>> spGetRequisitosPorTramite(spGetRequisitosPorTramite.Request request);
		Task<spSaveRequisito.Result> spSaveRequisito(spSaveRequisito.Request request);
		Task<spSaveDocumento.Result> spSaveDocumento(spSaveDocumento.Request request);
	}

}
