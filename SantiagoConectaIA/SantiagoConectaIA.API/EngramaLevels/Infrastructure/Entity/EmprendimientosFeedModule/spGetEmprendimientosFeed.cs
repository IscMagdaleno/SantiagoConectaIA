using EngramaCoreStandar.Dapper.Interfaces;
using EngramaCoreStandar.Dapper.Results;

namespace SantiagoConectaIA.API.EngramaLevels.Infrastructure.Entity.EmprendimientosFeedModule
{
    public class spGetEmprendimientosFeed
    {
        public class Request : SpRequest
        {
            public string StoredProcedure => "spGetEmprendimientosFeed";
            public int iPage { get; set; } = 1;
            public int iPageSize { get; set; } = 10;
            public string? vchSessionSeed { get; set; }
        }

        public class Result : DbResult
        {
            public bool bResult { get; set; }
            public string vchMessage { get; set; } = string.Empty;
            public string vchTipoEntidad { get; set; } = string.Empty;
            public int iIdEntidad { get; set; }
            public int iIdEmpresa { get; set; }
            public string vchTitulo { get; set; } = string.Empty;
            public string nvchDescripcion { get; set; } = string.Empty;
            public string vchImagenUrl { get; set; } = string.Empty;
            public string vchNombreEmpresa { get; set; } = string.Empty;
            public decimal mPrecio { get; set; }
            public bool bAplicaDescuento { get; set; }
            public decimal mPrecioDescuento { get; set; }
            public int iTotalRegistros { get; set; }
        }
    }
}
