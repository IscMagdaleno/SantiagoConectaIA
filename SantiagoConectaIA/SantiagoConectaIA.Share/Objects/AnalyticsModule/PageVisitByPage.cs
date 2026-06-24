using System;

namespace SantiagoConectaIA.Share.Objects.AnalyticsModule
{
    public class PageVisitByPage
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
