using SantiagoConectaIA.API.EngramaLevels.Infrastructure.Entity.EmprendimientosFeedModule;

namespace SantiagoConectaIA.API.EngramaLevels.Infrastructure.Interfaces
{
	public interface IEmprendimientosFeedRepository
	{
		Task<IEnumerable<spGetEmprendimientosFeed.Result>> spGetEmprendimientosFeed(spGetEmprendimientosFeed.Request daoModel);
		Task<IEnumerable<spSearchEmprendimientosFeed.Result>> spSearchEmprendimientosFeed(spSearchEmprendimientosFeed.Request daoModel);
	}
}
