using System;
using EngramaCoreStandar.Dapper.Interfaces;

namespace SantiagoConectaIA.API.EngramaLevels.Infrastructure.Entity.AnalyticsModule
{
    public class spSavePageVisit
    {
        public class Request : SpRequest
        {
            public string StoredProcedure => "spSavePageVisit";
            public string vchPageUrl { get; set; } = string.Empty;
            public string vchPageName { get; set; } = string.Empty;
            public string vchIpAddress { get; set; } = string.Empty;
            public string vchUserAgent { get; set; } = string.Empty;
            public string vchReferrer { get; set; } = string.Empty;
            public string vchBrowser { get; set; } = string.Empty;
            public string vchOperatingSystem { get; set; } = string.Empty;
            public string vchDeviceType { get; set; } = string.Empty;
        }

        public class Result : DbResult
        {
            public bool bResult { get; set; }
            public string vchMessage { get; set; } = string.Empty;
            public int iIdPageVisit { get; set; }
            public bool bIsUniqueVisitor { get; set; }
        }
    }
}
