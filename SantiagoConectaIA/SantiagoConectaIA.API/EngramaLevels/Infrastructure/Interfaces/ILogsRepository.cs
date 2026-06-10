using SantiagoConectaIA.API.EngramaLevels.Infrastructure.Entity.LogsModule;
using System.Threading.Tasks;

namespace SantiagoConectaIA.API.EngramaLevels.Infrastructure.Interfaces
{
    public interface ILogsRepository
    {
        Task<spSaveApiCallLog.Result> spSaveApiCallLog(spSaveApiCallLog.Request request);
    }
}
