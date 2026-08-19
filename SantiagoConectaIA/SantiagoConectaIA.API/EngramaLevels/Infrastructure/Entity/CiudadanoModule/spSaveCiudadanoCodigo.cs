using EngramaCoreStandar.Dapper.Interfaces;
using EngramaCoreStandar.Dapper.Results;

namespace SantiagoConectaIA.API.EngramaLevels.Infrastructure.Entity.CiudadanoModule
{
    public class spSaveCiudadanoCodigo
    {
        public class Request : SpRequest
        {
            public string StoredProcedure => "spSaveCiudadanoCodigo";
            public string vchTelefono { get; set; } = string.Empty;
            public string vchCodigo { get; set; } = string.Empty;
        }

        public class Result : DbResult
        {
            public bool bResult { get; set; }
            public string vchMessage { get; set; } = string.Empty;
        }
    }
}
