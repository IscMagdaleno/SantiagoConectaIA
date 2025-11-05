using EngramaCoreStandar.Results;

using SantiagoConectaIA.API.Helpers;
using SantiagoConectaIA.Share.Objects.OficinasModule;
using SantiagoConectaIA.Share.PostModels.OficinasModule;

namespace SantiagoConectaIA.API.EngramaLevels.Domain.Interfaces
{
	public interface IOficinasDomain
	{
		Task<Response<IEnumerable<Oficina>>> GetOficinas(PostGetOficinas postModel);
		Task<Response<PagedList<Oficina>>> SearchOficinas(PostSearchOficinas postModel);
		Task<Response<Oficina>> SaveOficina(PostSaveOficina postModel);

		Task<Response<IEnumerable<Dependencia>>> GetDependencias(PostGetDependencias postModel);
		Task<Response<Dependencia>> SaveDependencia(PostSaveDependencia postModel);

		Task<Response<OficinaPorTramite>> LinkOficinaTramite(PostLinkOficinaTramite postModel);
		Task<Response<IEnumerable<OficinaPorTramite>>> GetOficinasPorTramite(PostGetOficinasPorTramite postModel);
	}

}
