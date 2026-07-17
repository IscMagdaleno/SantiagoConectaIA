using SantiagoConectaIA.API.EngramaLevels.Infrastructure.Entity.InformacionLocalModule;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SantiagoConectaIA.API.EngramaLevels.Infrastructure.Interfaces
{
    public interface IInformacionLocalRepository
    {
        Task<IEnumerable<spGetInformacionLocal.Result>> spGetInformacionLocal(spGetInformacionLocal.Request daoModel);
        Task<spGetInformacionLocalById.Result> spGetInformacionLocalById(spGetInformacionLocalById.Request daoModel);
        Task<spSaveInformacionLocal.Result> spSaveInformacionLocal(spSaveInformacionLocal.Request daoModel);
    }
}
