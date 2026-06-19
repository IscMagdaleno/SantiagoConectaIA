using EngramaCoreStandar.Dapper.Interfaces;
using EngramaCoreStandar.Dapper.Results;

namespace SantiagoConectaIA.API.EngramaLevels.Infrastructure.Entity.EmpresasModule
{
    public class spSaveProductoServicio
    {
        public class Request : SpRequest
        {
            public string StoredProcedure => "spSaveProductoServicio";
            public int iIdProducto { get; set; }
            public int iIdCategoriaCat { get; set; }
            public string vchNombre { get; set; } = string.Empty;
            public string nvchDescripcionCorta { get; set; } = string.Empty;
            public decimal mPrecio { get; set; }
            public string vchImagenUrl { get; set; } = string.Empty;
            public bool bAplicaDescuento { get; set; }
            public decimal mPrecioDescuento { get; set; }
            public bool bEstatus { get; set; }
        }

        public class Result : DbResult
        {
            public bool bResult { get; set; }
            public string vchMessage { get; set; } = string.Empty;
            public int iIdProducto { get; set; }
        }
    }
}
