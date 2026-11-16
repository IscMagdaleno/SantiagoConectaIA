using SantiagoConectaIA.API.EngramaLevels.Infrastructure.Entity.PublicacionesRegistroModule;

namespace SantiagoConectaIA.API.EngramaLevels.Infrastructure.Interfaces
{
	public interface IPublicacionesRegistroRepository
	{
		Task<spSavePublicacionRegistro.Result> spSavePublicacionRegistro(spSavePublicacionRegistro.Request request);
		Task<IEnumerable<spGetPublicacionesRegistro.Result>> spGetPublicacionesRegistro(spGetPublicacionesRegistro.Request request);
	}
}
