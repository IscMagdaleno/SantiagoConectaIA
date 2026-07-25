using EngramaCoreStandar.Dapper.Interfaces;
using EngramaCoreStandar.Dapper.Results;

namespace SantiagoConectaIA.API.EngramaLevels.Infrastructure.Entity.EmpresasModule
{
    public class spGetEmpresaIdByCorreo
    {
        public class Request : SpRequest
        {
            public string StoredProcedure => "spGetEmpresaIdByCorreo";
            public string vchCorreo { get; set; }
        }

        public class Result : DbResult
        {
            public bool bResult { get; set; }
            public string vchMessage { get; set; }
            public int iIdEmpresa { get; set; }
            public int iIdPropietario { get; set; }
            public string vchNombre { get; set; }
            public string vchCorreo { get; set; }
            public string vchTelefono { get; set; }
            public string vchNombreComercial { get; set; }
        }
    }
}
