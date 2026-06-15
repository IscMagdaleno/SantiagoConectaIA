using EngramaCoreStandar.Dapper.Interfaces;
using EngramaCoreStandar.Dapper.Results;
using System;

namespace SantiagoConectaIA.API.EngramaLevels.Infrastructure.Entity.NoticiasModule
{
    public class spGetNoticias
    {
        public class Request : SpRequest
        {
            public string StoredProcedure => "spGetNoticias";
            public bool? bActivo { get; set; }
        }

        public class Result : DbResult
        {
            public bool bResult { get; set; }
            public string vchMessage { get; set; }
            public int iIdNoticia { get; set; }
            public string vchTitulo { get; set; }
            public string vchTituloEn { get; set; }
            public string nvchContenido { get; set; }
            public string nvchContenidoEn { get; set; }
            public string vchImagenPortada { get; set; }
            public DateTime dtFechaPublicacion { get; set; }
            public bool bActivo { get; set; }
            public int? iIdCategoria { get; set; }
            public string vchCategoriaNombre { get; set; }
            public string vchCategoriaColorHex { get; set; }
            public int? iIdNoticiaImagen { get; set; }
            public string vchUrlImagen { get; set; }
        }
    }
}
