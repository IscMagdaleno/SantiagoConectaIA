using System;

namespace SantiagoConectaIA.Share.PostModels.AnalyticsModule
{
    public class PostSavePageVisit
    {
        public int iIdPageVisit { get; set; }
        public string vchPageUrl { get; set; } = string.Empty;
        public string vchPageName { get; set; } = string.Empty;
        public string vchIpAddress { get; set; } = string.Empty;
        public string vchUserAgent { get; set; } = string.Empty;
        public string vchReferrer { get; set; } = string.Empty;
        public string vchBrowser { get; set; } = string.Empty;
        public string vchOperatingSystem { get; set; } = string.Empty;
        public string vchDeviceType { get; set; } = string.Empty;
    }
}
