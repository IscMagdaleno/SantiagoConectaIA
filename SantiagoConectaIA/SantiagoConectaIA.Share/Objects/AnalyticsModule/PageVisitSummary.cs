using System;

namespace SantiagoConectaIA.Share.Objects.AnalyticsModule
{
    public class PageVisitSummary
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
