using EngramaCoreStandar.Dapper.Interfaces;
using EngramaCoreStandar.Dapper.Results;

namespace SantiagoConectaIA.API.EngramaLevels.Infrastructure.Entity.PublicacionesCiudadanoModule
{
    public class spSavePublicacionCiudadano
    {
        public class Request : SpRequest
        {
            public string StoredProcedure => "spSavePublicacionCiudadano";
            public int iIdPublicacion { get; set; } = 0;
            public int iIdCiudadano { get; set; }
            public string? nvchTitulo { get; set; }
            public string nvchContenidoTexto { get; set; } = string.Empty;
            public string vchCategoriaPublicacion { get; set; } = "Comunidad";
        }

        public class Result : DbResult
        {
            public bool bResult { get; set; }
            public string vchMessage { get; set; } = string.Empty;
            public int iIdPublicacion { get; set; }
        }
    }
}
