using EngramaCoreStandar.Results;
using SantiagoConectaIA.Share.Objects.EmprendimientosFeedModule;
using SantiagoConectaIA.Share.PostModels.EmprendimientosFeedModule;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SantiagoConectaIA.API.EngramaLevels.Domain.Interfaces
{
    public interface IEmprendimientosFeedDomain
    {
        Task<Response<IEnumerable<EmprendimientosFeedCard>>> GetFeed(PostGetEmprendimientosFeed postModel);
        Task<Response<EmprendimientosFeedSearchResult>> SearchFeed(PostSearchEmprendimientosFeed postModel);
    }
}
