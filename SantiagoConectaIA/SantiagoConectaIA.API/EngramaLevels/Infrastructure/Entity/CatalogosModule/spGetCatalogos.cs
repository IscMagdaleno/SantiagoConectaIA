using EngramaCoreStandar.Dapper.Interfaces;
using EngramaCoreStandar.Dapper.Results;

namespace SantiagoConectaIA.API.EngramaLevels.Infrastructure.Entity.CatalogosModule
{
    public class spGetCatalogos
    {
        public class Request : SpRequest
        {
            public string StoredProcedure => "spGetCatalogos";
            public string vchGroupAlias { get; set; }
        }

        public class Result : DbResult
        {
            public bool bResult { get; set; }
            public string vchMessage { get; set; }
            
            public int IdCatalogo { get; set; }
            public int IdGrupo { get; set; }
            public string Descripcion { get; set; }
            public string Valor { get; set; }
        }
    }
}
