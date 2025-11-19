using SantiagoConectaIA.API.EngramaLevels.Infrastructure.Entity.OficinasModule;
// File: IOficinasRepository.cs

namespace SantiagoConectaIA.API.EngramaLevels.Infrastructure.Interfaces
{
	public interface IOficinasRepository
	{
		Task<IEnumerable<spGetOficinas.Result>> spGetOficinas(spGetOficinas.Request request);
		Task<spSearchOficinas.Result[]> spSearchOficinas(spSearchOficinas.Request request);
		Task<spSaveOficina.Result> spSaveOficina(spSaveOficina.Request request);


		Task<spLinkOficinaTramite.Result> spLinkOficinaTramite(spLinkOficinaTramite.Request request);
		Task<IEnumerable<spGetOficinasPorTramite.Result>> spGetOficinasPorTramite(spGetOficinasPorTramite.Request request);
	}

}
