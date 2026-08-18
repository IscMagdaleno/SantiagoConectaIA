using EngramaCoreStandar.Dapper.Interfaces;
using EngramaCoreStandar.Dapper.Results;

namespace SantiagoConectaIA.API.EngramaLevels.Infrastructure.Entity.CiudadanoModule
{
    public class spSaveCiudadano
    {
        public class Request : SpRequest
        {
            public string StoredProcedure => "spSaveCiudadano";
            public string vchAlias { get; set; } = string.Empty;
            public string vchTelefono { get; set; } = string.Empty;
            public string vchPassword { get; set; } = string.Empty;
        }

        public class Result : DbResult
        {
            public bool bResult { get; set; }
            public string vchMessage { get; set; } = string.Empty;
            public int iIdCiudadano { get; set; }
            public string vchAlias { get; set; } = string.Empty;
            public string vchTelefono { get; set; } = string.Empty;
        }
    }
}
