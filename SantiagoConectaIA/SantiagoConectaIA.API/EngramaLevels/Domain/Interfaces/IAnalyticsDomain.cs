using EngramaCoreStandar.Results;
using SantiagoConectaIA.Share.Objects.AnalyticsModule;
using SantiagoConectaIA.Share.PostModels.AnalyticsModule;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SantiagoConectaIA.API.EngramaLevels.Domain.Interfaces
{
    public interface IAnalyticsDomain
    {
        Task<Response<PageVisitSaveResult>> SavePageVisit(PostSavePageVisit postModel);
        Task<IEnumerable<PageVisitSummary>> GetPageVisitsSummary(DateTime? startDate, DateTime? endDate);
        Task<IEnumerable<PageVisitByPage>> GetPageVisitsByPage(DateTime? startDate, DateTime? endDate);
        Task<IEnumerable<DailyTraffic>> GetDailyTraffic(DateTime? startDate, DateTime? endDate);
        Task<IEnumerable<PageVisit>> GetRecentVisits(int topRows);
    }
}
