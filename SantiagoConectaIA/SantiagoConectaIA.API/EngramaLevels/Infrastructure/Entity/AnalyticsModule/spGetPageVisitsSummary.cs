using System;
using EngramaCoreStandar.Dapper.Interfaces;

namespace SantiagoConectaIA.API.EngramaLevels.Infrastructure.Entity.AnalyticsModule
{
    public class spGetPageVisitsSummary
    {
        public class Request : SpRequest
        {
            public string StoredProcedure => "spGetPageVisitsSummary";
            public DateTime? dtStartDate { get; set; }
            public DateTime? dtEndDate { get; set; }
        }

        public class Result : DbResult
        {
            public bool bResult { get; set; }
            public string vchMessage { get; set; } = string.Empty;
            public int TotalVisits { get; set; }
            public int UniqueVisitors { get; set; }
            public int NewVisitors { get; set; }
            public int ReturningVisitors { get; set; }
            public DateTime StartDate { get; set; }
            public DateTime EndDate { get; set; }
        }
    }
}
