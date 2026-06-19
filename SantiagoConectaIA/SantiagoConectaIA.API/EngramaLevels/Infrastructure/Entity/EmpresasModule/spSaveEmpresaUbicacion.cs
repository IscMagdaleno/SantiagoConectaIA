using EngramaCoreStandar.Dapper.Interfaces;
using EngramaCoreStandar.Dapper.Results;

namespace SantiagoConectaIA.API.EngramaLevels.Infrastructure.Entity.EmpresasModule
{
    public class spSaveEmpresaUbicacion
    {
        public class Request : SpRequest
        {
            public string StoredProcedure => "spSaveEmpresaUbicacion";
            public int iIdUbicacion { get; set; }
            public int iIdEmpresa { get; set; }
            public string vchAlias { get; set; } = string.Empty;
            public string vchDireccion { get; set; } = string.Empty;
            public double flLatitud { get; set; }
            public double flLongitud { get; set; }
            public bool bActivo { get; set; }
        }

        public class Result : DbResult
        {
            public bool bResult { get; set; }
            public string vchMessage { get; set; } = string.Empty;
            public int iIdUbicacion { get; set; }
        }
    }
}
