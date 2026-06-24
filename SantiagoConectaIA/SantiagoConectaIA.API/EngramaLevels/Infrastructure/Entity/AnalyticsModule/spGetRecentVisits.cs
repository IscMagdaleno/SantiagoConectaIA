using System;
using EngramaCoreStandar.Dapper.Interfaces;

namespace SantiagoConectaIA.API.EngramaLevels.Infrastructure.Entity.AnalyticsModule
{
    public class spGetRecentVisits
    {
        public class Request : SpRequest
        {
            public string StoredProcedure => "spGetRecentVisits";
            public int iTopRows { get; set; } = 100;
        }

        public class Result : DbResult
        {
            public bool bResult { get; set; }
            public string vchMessage { get; set; } = string.Empty;
            public int iIdPageVisit { get; set; }
            public string vchPageUrl { get; set; } = string.Empty;
            public string vchPageName { get; set; } = string.Empty;
            public string vchIpAddress { get; set; } = string.Empty;
            public string vchBrowser { get; set; } = string.Empty;
            public string vchOperatingSystem { get; set; } = string.Empty;
            public string vchDeviceType { get; set; } = string.Empty;
            public string vchReferrer { get; set; } = string.Empty;
            public bool bIsUniqueVisitor { get; set; }
            public DateTime dtVisitDate { get; set; }
        }
    }
}
