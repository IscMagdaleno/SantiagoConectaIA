using EngramaCoreStandar.Dapper.Interfaces;
using EngramaCoreStandar.Dapper.Results;

namespace SantiagoConectaIA.API.EngramaLevels.Infrastructure.Entity.EmpresasModule
{
    public class spSaveConfiguracionVisual
    {
        public class Request : SpRequest
        {
            public string StoredProcedure => "spSaveConfiguracionVisual";
            public int iIdConfiguracion { get; set; }
            public int iIdEmpresa { get; set; }
            public int? iIdPlantillaBase { get; set; }
            public string vchColorFondoBody { get; set; }
            public string vchColorTextoPrincipal { get; set; }
            public string vchColorTitulos { get; set; }
            public string vchColorBotones { get; set; }
            public string vchColorMargenFotos { get; set; }
            public string vchTipografiaTitulos { get; set; }
            public string vchTipografiaCuerpo { get; set; }
            public string vchEstiloBordes { get; set; }
        }

        public class Result : DbResult
        {
            public bool bResult { get; set; }
            public string vchMessage { get; set; } = string.Empty;
            public int iIdConfiguracion { get; set; }
        }
    }
}
