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


		Task<Response<Oficina>> LinkOficinaTramite(PostLinkOficinaTramite postModel);
		Task<Response<IEnumerable<Oficina>>> GetOficinasPorTramite(PostGetOficinasPorTramite postModel);
	}

}
