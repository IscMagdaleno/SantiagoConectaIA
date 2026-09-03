using EngramaCoreStandar.Dapper.Interfaces;
using EngramaCoreStandar.Dapper.Results;
using System;

namespace SantiagoConectaIA.API.EngramaLevels.Infrastructure.Entity.PublicacionesCiudadanoModule
{
    public class spGetPublicacionesCiudadano
    {
        public class Request : SpRequest
        {
            public string StoredProcedure => "spGetPublicacionesCiudadano";
            public int iPage { get; set; } = 1;
            public int iPageSize { get; set; } = 10;
            public string? vchCategoria { get; set; }
            public string? nvchBusqueda { get; set; }
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
