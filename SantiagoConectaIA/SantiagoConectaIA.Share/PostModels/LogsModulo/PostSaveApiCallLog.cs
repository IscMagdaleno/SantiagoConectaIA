using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SantiagoConectaIA.Share.PostModels.LogsModulo
{
    public class PostSaveApiCallLog
    {
        public int iIdApiCallLog { get; set; }
        public string vchEndpoint { get; set; }
        public string vchRequestMethod { get; set; }
        public DateTime dtRequestTimestamp { get; set; }
        public string nvchRequestBody { get; set; }
        public DateTime dtResponseTimestamp { get; set; }
        public string nvchResponseBody { get; set; }
        public bool bIsSuccess { get; set; }
        public int iDurationMs { get; set; }
        public string vchHost { get; set; }
    }
}
