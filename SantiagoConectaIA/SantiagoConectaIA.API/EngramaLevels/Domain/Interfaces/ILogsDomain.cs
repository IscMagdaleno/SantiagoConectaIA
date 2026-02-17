using EngramaCoreStandar.Results;
using SantiagoConectaIA.Share.Objects.LogsModulo;
using SantiagoConectaIA.Share.PostModels.LogsModulo;
using System.Threading.Tasks;

namespace SantiagoConectaIA.API.EngramaLevels.Domain.Interfaces
{
    public interface ILogsDomain
    {
        Task<Response<ApiCallLog>> SaveApiCallLog(PostSaveApiCallLog postModel);
    }
}
