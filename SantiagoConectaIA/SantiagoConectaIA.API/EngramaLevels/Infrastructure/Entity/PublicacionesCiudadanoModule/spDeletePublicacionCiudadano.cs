using EngramaCoreStandar.Dapper.Interfaces;
using EngramaCoreStandar.Dapper.Results;

namespace SantiagoConectaIA.API.EngramaLevels.Infrastructure.Entity.PublicacionesCiudadanoModule
{
    public class spDeletePublicacionCiudadano
    {
        public class Request : SpRequest
        {
            public string StoredProcedure => "spDeletePublicacionCiudadano";
            public int iIdPublicacion { get; set; }
            public int iIdCiudadano { get; set; }
        }

        public class Result : DbResult
        {
            public bool bResult { get; set; }
            public string vchMessage { get; set; } = string.Empty;
        }
    }
}
