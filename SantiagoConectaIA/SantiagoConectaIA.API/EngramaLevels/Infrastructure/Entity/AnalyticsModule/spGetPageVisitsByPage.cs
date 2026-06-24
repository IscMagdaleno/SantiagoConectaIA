using System;
using EngramaCoreStandar.Dapper.Interfaces;

namespace SantiagoConectaIA.API.EngramaLevels.Infrastructure.Entity.AnalyticsModule
{
    public class spGetPageVisitsByPage
    {
        public class Request : SpRequest
        {
            public string StoredProcedure => "spGetPageVisitsByPage";
            public DateTime? dtStartDate { get; set; }
            public DateTime? dtEndDate { get; set; }
        }

        public class Result : DbResult
        {
            public bool bResult { get; set; }
            public string vchMessage { get; set; } = string.Empty;
            public string vchPageUrl { get; set; } = string.Empty;
            public string vchPageName { get; set; } = string.Empty;
            public int TotalVisits { get; set; }
            public int UniqueVisitors { get; set; }
            public DateTime dtLastVisit { get; set; }
        }
    }
}
