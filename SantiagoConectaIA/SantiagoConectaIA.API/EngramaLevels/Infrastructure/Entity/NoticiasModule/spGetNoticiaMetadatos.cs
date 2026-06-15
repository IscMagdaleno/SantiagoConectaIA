using EngramaCoreStandar.Dapper.Interfaces;
using EngramaCoreStandar.Dapper.Results;

namespace SantiagoConectaIA.API.EngramaLevels.Infrastructure.Entity.NoticiasModule
{
    public class spGetNoticiaMetadatos
    {
        public class Request : SpRequest
        {
            public string StoredProcedure => "spGetNoticiaMetadatos";
            public bool? bActivo { get; set; }
        }

        public class Result : DbResult
        {
            public bool bResult { get; set; }
            public string vchMessage { get; set; }

            public int iIdMetadato { get; set; }
            public int iIdNoticia { get; set; }
            public int iIdFila { get; set; }
            public int iFilaOrden { get; set; }
            public int iIdTipoDato { get; set; }
            public string vchTitulo { get; set; }
            public string nvchValor { get; set; }
            public int iMetadatoOrden { get; set; }
            public int? iAncho { get; set; }
            public string vchAlineacion { get; set; }
            public string vchAlto { get; set; }
            public string nvchConfiguracion { get; set; }
        }
    }
}
