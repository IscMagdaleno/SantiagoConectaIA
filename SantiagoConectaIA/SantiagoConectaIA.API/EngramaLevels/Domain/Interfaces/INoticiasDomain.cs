using EngramaCoreStandar.Results;

using SantiagoConectaIA.Share.Objects.NoticiasModule;
using SantiagoConectaIA.Share.PostModels.NoticiasModule;

namespace SantiagoConectaIA.API.EngramaLevels.Domain.Interfaces
{
	public interface INoticiasDomain
	{
		Task<Response<IEnumerable<Noticia>>> GetNoticias(PostGetNoticias postModel);
		Task<Response<Noticia>> SaveNoticia(PostSaveNoticia postModel);
	}
}
