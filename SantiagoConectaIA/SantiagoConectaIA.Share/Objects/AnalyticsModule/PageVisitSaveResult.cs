using System;

namespace SantiagoConectaIA.Share.Objects.AnalyticsModule
{
    public class PageVisitSaveResult
    {
        public bool bResult { get; set; }
        public string vchMessage { get; set; } = string.Empty;
        public int iIdPageVisit { get; set; }
        public bool bIsUniqueVisitor { get; set; }
    }
}
