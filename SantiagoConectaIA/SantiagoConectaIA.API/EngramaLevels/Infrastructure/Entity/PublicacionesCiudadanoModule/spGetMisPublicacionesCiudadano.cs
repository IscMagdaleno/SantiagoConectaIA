using EngramaCoreStandar.Dapper.Interfaces;
using EngramaCoreStandar.Dapper.Results;
using System;

namespace SantiagoConectaIA.API.EngramaLevels.Infrastructure.Entity.PublicacionesCiudadanoModule
{
    public class spGetMisPublicacionesCiudadano
    {
        public class Request : SpRequest
        {
            public string StoredProcedure => "spGetMisPublicacionesCiudadano";
            public int iIdCiudadano { get; set; }
            public int iPage { get; set; } = 1;
            public int iPageSize { get; set; } = 20;
        }

        public class Result : DbResult
        {
            public bool bResult { get; set; }
            public string vchMessage { get; set; } = string.Empty;
            public int iIdPublicacion { get; set; }
            public int iIdCiudadano { get; set; }
            public string vchAliasCiudadano { get; set; } = string.Empty;
            public string? vchAvatarCiudadano { get; set; }
            public bool bCiudadanoVerificado { get; set; }
            public string? nvchTitulo { get; set; }
            public string nvchContenidoTexto { get; set; } = string.Empty;
            public string vchCategoriaPublicacion { get; set; } = string.Empty;
            public DateTime dtFechaCreacion { get; set; }
            public bool bActiva { get; set; }
            public int iTotalComentarios { get; set; }
            public string? nvchImagenesJson { get; set; }
            public string? vchPrimeraImagenUrl { get; set; }
            public int iTotalRegistros { get; set; }
        }
    }
}
