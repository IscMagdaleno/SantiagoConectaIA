using System;
using EngramaCoreStandar.Dapper.Interfaces;
using EngramaCoreStandar.Dapper.Results;

namespace SantiagoConectaIA.API.EngramaLevels.Infrastructure.Entity.CatalogosModule
{
    public class spGetParametro
    {
        public class Request : SpRequest
        {
            public string StoredProcedure => "spGetParametro";
            public string vchAlias { get; set; }
        }

        public class Result : DbResult
        {
            public bool bResult { get; set; }
            public string vchMessage { get; set; }

            public int IIdParametro { get; set; }
            public int? IIdParametroPadre { get; set; }
            public string NvchAlias { get; set; }
            public string NvchNombre { get; set; }
            public string NvchNombreEn { get; set; }
            public string NvchDescripcion { get; set; }
            public string NvchDescripcionEn { get; set; }
            public int ISecuencia { get; set; }
            public bool BTieneValores { get; set; }
            public string NvchValor1 { get; set; }
            public string NvchValor2 { get; set; }
            public bool BHabilitado { get; set; }
        }
    }
}
