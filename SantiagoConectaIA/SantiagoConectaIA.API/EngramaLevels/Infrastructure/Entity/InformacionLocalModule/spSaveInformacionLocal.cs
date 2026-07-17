using EngramaCoreStandar.Dapper.Interfaces;
using EngramaCoreStandar.Dapper.Results;

namespace SantiagoConectaIA.API.EngramaLevels.Infrastructure.Entity.InformacionLocalModule
{
    public class spSaveInformacionLocal
    {
        public class Request : SpRequest
        {
            public string StoredProcedure => "spSaveInformacionLocal";
            public int iIdInformacionLocal { get; set; }
            public string nvchCategoria { get; set; }
            public string nvchTitulo { get; set; }
            public string? nvchPalabrasClave { get; set; }
            public string nvchDescripcionCorta { get; set; }
            public string? nvchContenidoDetallado { get; set; }
            public string? nvchUbicacion_LatLong { get; set; }
            public bool bActivo { get; set; }
        }

        public class Result : DbResult
        {
            public bool bResult { get; set; }
            public string vchMessage { get; set; }
            public int iIdInformacionLocal { get; set; }
        }
    }
}
