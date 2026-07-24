using EngramaCoreStandar.Dapper.Interfaces;
using System;

namespace SantiagoConectaIA.API.EngramaLevels.Infrastructure.Entities
{
    public class spGetInformacionLocal
    {
        public class Request : SpRequest
        {
            public string StoredProcedure => "spGetInformacionLocal";
            public bool bActivo { get; set; }
        }

        public class Result : DbResult
        {
            public bool bResult { get; set; }
            public string vchMessage { get; set; }
            
            public int iIdInformacionLocal { get; set; }
            public string nvchCategoria { get; set; }
            public string nvchTitulo { get; set; }
            public string nvchPalabrasClave { get; set; }
            public string nvchDescripcionCorta { get; set; }
            public string nvchContenidoDetallado { get; set; }
            public string nvchUbicacion_LatLong { get; set; }
            public DateTime dtFechaCreacion { get; set; }
            public bool bActivo { get; set; }
        }
    }
}
