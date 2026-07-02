using EngramaCoreStandar.Dapper.Interfaces;
using EngramaCoreStandar.Dapper.Results;

namespace SantiagoConectaIA.API.EngramaLevels.Infrastructure.Entity.EventosModule
{
    public class spGetImagenesRegistro
    {
        public class Request : SpRequest
        {
            public string StoredProcedure => "spGetImagenesRegistro";
            public string vchTablaOrigen { get; set; }
            public int iIdRegistro { get; set; }
        }

        public class Result : DbResult
        {
            public bool bResult { get; set; }
            public string vchMessage { get; set; }
            public int iIdImagen { get; set; }
            public string vchTablaOrigen { get; set; }
            public int iIdRegistro { get; set; }
            public string vchUrlImagen { get; set; }
            public string vchDescripcion { get; set; }
            public int iOrden { get; set; }
        }
    }
}
