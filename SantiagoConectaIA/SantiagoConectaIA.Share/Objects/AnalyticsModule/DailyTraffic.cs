using System;

namespace SantiagoConectaIA.Share.Objects.AnalyticsModule
{
    public class DailyTraffic
    {
        public bool bResult { get; set; }
        public string vchMessage { get; set; } = string.Empty;
        public DateTime dtVisitDay { get; set; }
        public int TotalVisits { get; set; }
        public int UniqueVisitors { get; set; }
        public int NewVisitors { get; set; }
    }
}
