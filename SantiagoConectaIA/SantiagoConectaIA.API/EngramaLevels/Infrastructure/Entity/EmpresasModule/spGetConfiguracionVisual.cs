using EngramaCoreStandar.Dapper.Interfaces;
using EngramaCoreStandar.Dapper.Results;

namespace SantiagoConectaIA.API.EngramaLevels.Infrastructure.Entity.EmpresasModule
{
    public class spGetConfiguracionVisual
    {
        public class Request : SpRequest
        {
            public string StoredProcedure => "spGetConfiguracionVisual";
            public int iIdEmpresa { get; set; }
        }

        public class Result : DbResult
        {
            public bool bResult { get; set; }
            public string vchMessage { get; set; } = string.Empty;
            public int iIdConfiguracion { get; set; }
            public int iIdEmpresa { get; set; }
            public int? iIdPlantillaBase { get; set; }
            public string vchColorFondoBody { get; set; } = string.Empty;
            public string vchColorTextoPrincipal { get; set; } = string.Empty;
            public string vchColorTitulos { get; set; } = string.Empty;
            public string vchColorBotones { get; set; } = string.Empty;
            public string vchColorMargenFotos { get; set; } = string.Empty;
            public string vchTipografiaTitulos { get; set; } = string.Empty;
            public string vchTipografiaCuerpo { get; set; } = string.Empty;
            public string vchEstiloBordes { get; set; } = string.Empty;
        }
    }
}
