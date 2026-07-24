using EngramaCoreStandar.Dapper.Interfaces;
using EngramaCoreStandar.Dapper.Results;

namespace SantiagoConectaIA.API.EngramaLevels.Infrastructure.Entity.EmpresasModule
{
    public class spSaveEmprendimientoCompleto
    {
        public class Request : SpRequest
        {
            public string StoredProcedure => "spSaveEmprendimientoCompleto";

            public int iIdEmpresa { get; set; }
            public int? iIdPropietario { get; set; }
            public int? iIdCatalogoEmpresa { get; set; }
            public string vchNombreComercial { get; set; }
            public string vchSlogan { get; set; }
            public string vchLogoUrl { get; set; }
            public string nvchDescripcion { get; set; }
            public string nvchMision { get; set; }
            public string nvchVision { get; set; }
            public string nvchHistoria { get; set; }
            public string vchTelefono { get; set; }
            public string vchCorreo { get; set; }
            public bool bEstatus { get; set; }
            public string vchJsonDetalles { get; set; }
        }

        public class Result : DbResult
        {
            public bool bResult { get; set; }
            public string vchMessage { get; set; }
            public int iIdEmpresa { get; set; }
        }
    }
}
