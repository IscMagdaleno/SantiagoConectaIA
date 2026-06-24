using EngramaCoreStandar.Dapper;
using SantiagoConectaIA.API.EngramaLevels.Infrastructure.Entity.AnalyticsModule;
using SantiagoConectaIA.API.EngramaLevels.Infrastructure.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SantiagoConectaIA.API.EngramaLevels.Infrastructure.Repository
{
    public class AnalyticsRepository : IAnalyticsRepository
    {
        private readonly IDapperManagerHelper _managerHelper;

        public AnalyticsRepository(IDapperManagerHelper managerHelper)
        {
            _managerHelper = managerHelper;
        }

        public async Task<spSavePageVisit.Result> spSavePageVisit(spSavePageVisit.Request request)
        {
            var res = await _managerHelper.GetAsync<spSavePageVisit.Result, spSavePageVisit.Request>(request, "", "SCIA");
            if (res.Ok && res.Data != null) return res.Data;
            return new spSavePageVisit.Result { bResult = false, vchMessage = res.Msg };
        }

        public async Task<IEnumerable<spGetPageVisitsSummary.Result>> spGetPageVisitsSummary(spGetPageVisitsSummary.Request request)
        {
            var respuesta = await _managerHelper.GetAllAsync<spGetPageVisitsSummary.Result, spGetPageVisitsSummary.Request>(request, "", "SCIA");
            if (respuesta.Ok) return respuesta.Data;
            return new List<spGetPageVisitsSummary.Result> { new() { bResult = false, vchMessage = respuesta.Msg } };
        }

        public async Task<IEnumerable<spGetPageVisitsByPage.Result>> spGetPageVisitsByPage(spGetPageVisitsByPage.Request request)
        {
            var respuesta = await _managerHelper.GetAllAsync<spGetPageVisitsByPage.Result, spGetPageVisitsByPage.Request>(request, "", "SCIA");
            if (respuesta.Ok) return respuesta.Data;
            return new List<spGetPageVisitsByPage.Result> { new() { bResult = false, vchMessage = respuesta.Msg } };
        }

        public async Task<IEnumerable<spGetDailyTraffic.Result>> spGetDailyTraffic(spGetDailyTraffic.Request request)
        {
            var respuesta = await _managerHelper.GetAllAsync<spGetDailyTraffic.Result, spGetDailyTraffic.Request>(request, "", "SCIA");
            if (respuesta.Ok) return respuesta.Data;
            return new List<spGetDailyTraffic.Result> { new() { bResult = false, vchMessage = respuesta.Msg } };
        }

        public async Task<IEnumerable<spGetRecentVisits.Result>> spGetRecentVisits(spGetRecentVisits.Request request)
        {
            var respuesta = await _managerHelper.GetAllAsync<spGetRecentVisits.Result, spGetRecentVisits.Request>(request, "", "SCIA");
            if (respuesta.Ok) return respuesta.Data;
            return new List<spGetRecentVisits.Result> { new() { bResult = false, vchMessage = respuesta.Msg } };
        }
    }
}
