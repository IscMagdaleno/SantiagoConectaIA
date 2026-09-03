using EngramaCoreStandar.Dapper.Interfaces;
using EngramaCoreStandar.Dapper.Results;
using System;

namespace SantiagoConectaIA.API.EngramaLevels.Infrastructure.Entity.FeedModule
{
    public class spGetFeed
    {
        public class Request : SpRequest
        {
            public string StoredProcedure => "spGetFeed";
            public int iPage { get; set; } = 1;
            public int iPageSize { get; set; } = 10;
            public string? vchSessionSeed { get; set; }
            public string? vchTipoFiltro { get; set; }
        }

        public class Result : DbResult
        {
            public bool bResult { get; set; }
            public string vchMessage { get; set; } = string.Empty;
            public string vchTipoEntidad { get; set; } = string.Empty;
            public int iIdEntidad { get; set; }
            public string vchTitulo { get; set; } = string.Empty;
            public string nvchDescripcion { get; set; } = string.Empty;
            public string? nvchContenidoDetallado { get; set; }
            public string vchImagenUrl { get; set; } = string.Empty;
            public DateTime? dtFecha { get; set; }
            public int iTotalRegistros { get; set; }
            public string? nvchImagenesJson { get; set; }
        }
    }
}
