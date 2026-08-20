using SantiagoConectaIA.API.EngramaLevels.Infrastructure.Entity.OpinionModule;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SantiagoConectaIA.API.EngramaLevels.Infrastructure.Interfaces
{
    public interface IOpinionRepository
    {
        Task<IEnumerable<spGetOpiniones.Result>> spGetOpiniones(spGetOpiniones.Request daoModel);
        Task<spSaveOpinion.Result> spSaveOpinion(spSaveOpinion.Request daoModel);
    }
}
