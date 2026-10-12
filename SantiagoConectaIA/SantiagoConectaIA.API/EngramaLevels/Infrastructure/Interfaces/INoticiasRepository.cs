using SantiagoConectaIA.API.EngramaLevels.Infrastructure.Entity.NoticiasModule;

namespace SantiagoConectaIA.API.EngramaLevels.Infrastructure.Interfaces
{
	public interface INoticiasRepository
	{
		Task<IEnumerable<spGetNoticias.Result>> spGetNoticias(spGetNoticias.Request daoModel);
		Task<spSaveNoticia.Result> spSaveNoticia(spSaveNoticia.Request daoModel);
		Task<IEnumerable<spGetNoticiaMetadatos.Result>> spGetNoticiaMetadatos(spGetNoticiaMetadatos.Request daoModel);
	}
}
