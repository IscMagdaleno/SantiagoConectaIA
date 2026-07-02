using EngramaCoreStandar.Dapper.Interfaces;
using EngramaCoreStandar.Dapper.Results;

namespace SantiagoConectaIA.API.EngramaLevels.Infrastructure.Entity.EventosModule
{
    public class spDeleteImagenRegistro
    {
        public class Request : SpRequest
        {
            public string StoredProcedure => "spDeleteImagenRegistro";
            public int iIdImagen { get; set; }
        }

        public class Result : DbResult
        {
            public bool bResult { get; set; }
            public string vchMessage { get; set; }
        }
    }
}
