using EngramaCoreStandar.Dapper;
using SantiagoConectaIA.API.EngramaLevels.Infrastructure.Entity.LogsModule;
using SantiagoConectaIA.API.EngramaLevels.Infrastructure.Interfaces;
using System.Threading.Tasks;

namespace SantiagoConectaIA.API.EngramaLevels.Infrastructure.Repository
{
    public class LogsRepository : ILogsRepository
    {
        private readonly IDapperManagerHelper _managerHelper;

        public LogsRepository(IDapperManagerHelper managerHelper)
        {
            _managerHelper = managerHelper;
        }

        public async Task<spSaveApiCallLog.Result> spSaveApiCallLog(spSaveApiCallLog.Request request)
        {
            var res = await _managerHelper.GetAsync<spSaveApiCallLog.Result, spSaveApiCallLog.Request>(request, "");
            if (res.Ok && res.Data != null) return res.Data;
            return new spSaveApiCallLog.Result { bResult = false, vchMessage = res.Msg };
        }
    }
}
