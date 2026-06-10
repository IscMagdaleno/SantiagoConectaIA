using EngramaCoreStandar.Dapper.Interfaces;
using EngramaCoreStandar.Dapper.Results;

namespace SantiagoConectaIA.API.EngramaLevels.Infrastructure.Entity.NoticiasModule
{
    public class spSaveNoticiaImagen
    {
        public class Request : SpRequest
        {
            public string StoredProcedure => "spSaveNoticiaImagen";
            public int iIdNoticia { get; set; }
            public string vchUrlImagen { get; set; }
        }

        public class Result : DbResult
        {
            public bool bResult { get; set; }
            public string vchMessage { get; set; }
            public int iIdNoticiaImagen { get; set; }
        }
    }
}
