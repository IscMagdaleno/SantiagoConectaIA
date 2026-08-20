using EngramaCoreStandar.Dapper.Interfaces;
using EngramaCoreStandar.Dapper.Results;
using System;

namespace SantiagoConectaIA.API.EngramaLevels.Infrastructure.Entity.OpinionModule
{
    public class spGetOpiniones
    {
        public class Request : SpRequest
        {
            public string StoredProcedure => "spGetOpiniones";
            public string vchTipoEntidad { get; set; } = string.Empty;
            public int iIdEntidad { get; set; }
        }

        public class Result : DbResult
        {
            public bool bResult { get; set; }
            public string vchMessage { get; set; } = string.Empty;
            public int iIdOpinion { get; set; }
            public string vchTipoEntidad { get; set; } = string.Empty;
            public int iIdEntidad { get; set; }
            public int iIdCiudadano { get; set; }
            public string vchAlias { get; set; } = string.Empty;
            public int? iIdOpinionPadre { get; set; }
            public string nvchTexto { get; set; } = string.Empty;
            public DateTime? dtFechaCreacion { get; set; }
        }
    }
}
