using EngramaCoreStandar.Mapper;
using EngramaCoreStandar.Results;
using SantiagoConectaIA.API.EngramaLevels.Domain.Interfaces;
using SantiagoConectaIA.API.EngramaLevels.Infrastructure.Entity.AnalyticsModule;
using SantiagoConectaIA.API.EngramaLevels.Infrastructure.Interfaces;
using SantiagoConectaIA.Share.Objects.AnalyticsModule;
using SantiagoConectaIA.Share.PostModels.AnalyticsModule;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SantiagoConectaIA.API.EngramaLevels.Domain.Core
{
    public class AnalyticsDomain : IAnalyticsDomain
    {
        private readonly IAnalyticsRepository _analyticsRepository;
        private readonly MapperHelper _mapperHelper;
        private readonly IResponseHelper _responseHelper;

        public AnalyticsDomain(IAnalyticsRepository analyticsRepository, MapperHelper mapperHelper, IResponseHelper responseHelper)
        {
            _analyticsRepository = analyticsRepository;
            _mapperHelper = mapperHelper;
            _responseHelper = responseHelper;
        }

        public async Task<Response<PageVisitSaveResult>> SavePageVisit(PostSavePageVisit postModel)
        {
            try
            {
                var model = _mapperHelper.Get<PostSavePageVisit, spSavePageVisit.Request>(postModel);
                var result = await _analyticsRepository.spSavePageVisit(model);
                var validation = _responseHelper.Validacion<spSavePageVisit.Result, PageVisitSaveResult>(result);

                if (validation.IsSuccess)
                {
                    validation.Data.iIdPageVisit = result.iIdPageVisit;
                    validation.Data.bIsUniqueVisitor = result.bIsUniqueVisitor;
                }

                return validation;
            }
            catch (Exception ex)
            {
                return Response<PageVisitSaveResult>.BadResult(ex.Message, new PageVisitSaveResult());
            }
        }

        public async Task<IEnumerable<PageVisitSummary>> GetPageVisitsSummary(DateTime? startDate, DateTime? endDate)
        {
            var request = new spGetPageVisitsSummary.Request
            {
                dtStartDate = startDate,
                dtEndDate = endDate
            };
            var results = await _analyticsRepository.spGetPageVisitsSummary(request);
            return results.Select(r => new PageVisitSummary
            {
                bResult = r.bResult,
                vchMessage = r.vchMessage,
                TotalVisits = r.TotalVisits,
                UniqueVisitors = r.UniqueVisitors,
                NewVisitors = r.NewVisitors,
                ReturningVisitors = r.ReturningVisitors,
                StartDate = r.StartDate,
                EndDate = r.EndDate
            });
        }

        public async Task<IEnumerable<PageVisitByPage>> GetPageVisitsByPage(DateTime? startDate, DateTime? endDate)
        {
            var request = new spGetPageVisitsByPage.Request
            {
                dtStartDate = startDate,
                dtEndDate = endDate
            };
            var results = await _analyticsRepository.spGetPageVisitsByPage(request);
            return results.Select(r => new PageVisitByPage
            {
                bResult = r.bResult,
                vchMessage = r.vchMessage,
                vchPageUrl = r.vchPageUrl,
                vchPageName = r.vchPageName,
                TotalVisits = r.TotalVisits,
                UniqueVisitors = r.UniqueVisitors,
                dtLastVisit = r.dtLastVisit
            });
        }

        public async Task<IEnumerable<DailyTraffic>> GetDailyTraffic(DateTime? startDate, DateTime? endDate)
        {
            var request = new spGetDailyTraffic.Request
            {
                dtStartDate = startDate,
                dtEndDate = endDate
            };
            var results = await _analyticsRepository.spGetDailyTraffic(request);
            return results.Select(r => new DailyTraffic
            {
                bResult = r.bResult,
                vchMessage = r.vchMessage,
                dtVisitDay = r.dtVisitDay,
                TotalVisits = r.TotalVisits,
                UniqueVisitors = r.UniqueVisitors,
                NewVisitors = r.NewVisitors
            });
        }

        public async Task<IEnumerable<PageVisit>> GetRecentVisits(int topRows)
        {
            var request = new spGetRecentVisits.Request
            {
                iTopRows = topRows
            };
            var results = await _analyticsRepository.spGetRecentVisits(request);
            return results.Select(r => new PageVisit
            {
                iIdPageVisit = r.iIdPageVisit,
                vchPageUrl = r.vchPageUrl,
                vchPageName = r.vchPageName,
                vchIpAddress = r.vchIpAddress,
                vchBrowser = r.vchBrowser,
                vchOperatingSystem = r.vchOperatingSystem,
                vchDeviceType = r.vchDeviceType,
                vchReferrer = r.vchReferrer,
                bIsUniqueVisitor = r.bIsUniqueVisitor,
                dtVisitDate = r.dtVisitDate
            });
        }
    }
}
