using SantiagoConectaIA.API.EngramaLevels.Infrastructure.Entity.EmprendimientosFeedModule;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SantiagoConectaIA.API.EngramaLevels.Infrastructure.Interfaces
{
    public interface IEmprendimientosFeedRepository
    {
        Task<IEnumerable<spGetEmprendimientosFeed.Result>> spGetEmprendimientosFeed(spGetEmprendimientosFeed.Request daoModel);
        Task<IEnumerable<spSearchEmprendimientosFeed.Result>> spSearchEmprendimientosFeed(spSearchEmprendimientosFeed.Request daoModel);
    }
}
