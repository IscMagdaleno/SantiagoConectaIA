using SantiagoConectaIA.API.EngramaLevels.Infrastructure.Entity.MemesModule;

namespace SantiagoConectaIA.API.EngramaLevels.Infrastructure.Interfaces
{
	public interface IMemesRepository
	{
		Task<spSaveMemeCreado.Result> spSaveMemeCreado(spSaveMemeCreado.Request request);
		Task<IEnumerable<spGetMemesCreados.Result>> spGetMemesCreados(spGetMemesCreados.Request request);
		Task<spGetMemesResumen.Result> spGetMemesResumen(spGetMemesResumen.Request request);
		Task<spDescartarMeme.Result> spDescartarMeme(spDescartarMeme.Request request);
	}
}
