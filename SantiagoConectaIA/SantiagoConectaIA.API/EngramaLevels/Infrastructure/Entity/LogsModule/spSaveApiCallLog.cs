using EngramaCoreStandar.Dapper.Interfaces;
using System;

namespace SantiagoConectaIA.API.EngramaLevels.Infrastructure.Entity.LogsModule
{
    public class spSaveApiCallLog
    {
        public class Request : SpRequest
        {
            public string StoredProcedure => "spSaveApiCallLog";
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

        public class Result : DbResult
        {
            public bool bResult { get; set; }
            public string vchMessage { get; set; }
            public int iIdApiCallLog { get; set; }
        }
    }
}
