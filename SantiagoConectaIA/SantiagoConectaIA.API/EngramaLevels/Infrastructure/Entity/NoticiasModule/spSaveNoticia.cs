using EngramaCoreStandar.Dapper.Interfaces;
using EngramaCoreStandar.Dapper.Results;
using System;

namespace SantiagoConectaIA.API.EngramaLevels.Infrastructure.Entity.NoticiasModule
{
    public class spSaveNoticia
    {
        public class Request : SpRequest
        {
            public string StoredProcedure => "spSaveNoticia";
            public int iIdNoticia { get; set; }
            public string vchTitulo { get; set; }
            public string vchTituloEn { get; set; }
            public string nvchContenido { get; set; }
            public string nvchContenidoEn { get; set; }
            public string vchImagenPortada { get; set; }
            public DateTime dtFechaPublicacion { get; set; }
            public bool bActivo { get; set; }
            public int? iIdCategoria { get; set; }
            public string jsonImagenes { get; set; }
            public string jsonFilas { get; set; }
        }

        public class Result : DbResult
        {
            public bool bResult { get; set; }
            public string vchMessage { get; set; }
            public int iIdNoticia { get; set; }
        }
    }
}
