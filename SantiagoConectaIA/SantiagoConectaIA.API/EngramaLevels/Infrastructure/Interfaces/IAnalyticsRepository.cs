using SantiagoConectaIA.API.EngramaLevels.Infrastructure.Entity.AnalyticsModule;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SantiagoConectaIA.API.EngramaLevels.Infrastructure.Interfaces
{
    public interface IAnalyticsRepository
    {
        Task<spSavePageVisit.Result> spSavePageVisit(spSavePageVisit.Request request);
        Task<IEnumerable<spGetPageVisitsSummary.Result>> spGetPageVisitsSummary(spGetPageVisitsSummary.Request request);
        Task<IEnumerable<spGetPageVisitsByPage.Result>> spGetPageVisitsByPage(spGetPageVisitsByPage.Request request);
        Task<IEnumerable<spGetDailyTraffic.Result>> spGetDailyTraffic(spGetDailyTraffic.Request request);
        Task<IEnumerable<spGetRecentVisits.Result>> spGetRecentVisits(spGetRecentVisits.Request request);
    }
}
