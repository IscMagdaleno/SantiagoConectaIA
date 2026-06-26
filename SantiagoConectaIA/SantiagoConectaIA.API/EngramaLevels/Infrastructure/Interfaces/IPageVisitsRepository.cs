using System.Collections.Generic;
using System.Threading.Tasks;
using SantiagoConectaIA.API.EngramaLevels.Infrastructure.Entity.PageVisitsModule;

namespace SantiagoConectaIA.API.EngramaLevels.Infrastructure.Interfaces
{
	public interface IPageVisitsRepository
	{
		Task<IEnumerable<spGetPageVisitsStats.Result>> spGetPageVisitsStats(spGetPageVisitsStats.Request daoModel);
	}
}
