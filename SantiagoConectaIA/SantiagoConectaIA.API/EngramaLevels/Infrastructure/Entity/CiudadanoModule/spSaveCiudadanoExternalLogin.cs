using EngramaCoreStandar.Dapper.Interfaces;
using EngramaCoreStandar.Dapper.Results;

namespace SantiagoConectaIA.API.EngramaLevels.Infrastructure.Entity.CiudadanoModule
{
    public class spSaveCiudadanoExternalLogin
    {
        public class Request : SpRequest
        {
            public string StoredProcedure => "spSaveCiudadanoExternalLogin";
            public string vchProveedor { get; set; } = string.Empty;
            public string vchIdProveedor { get; set; } = string.Empty;
            public string? vchEmail { get; set; }
            public string? vchAlias { get; set; }
            public string? vchAvatarUrl { get; set; }
        }

        public class Result : DbResult
        {
            public bool bResult { get; set; }
            public string vchMessage { get; set; } = string.Empty;
            public int iIdCiudadano { get; set; }
            public string vchAlias { get; set; } = string.Empty;
            public string vchTelefono { get; set; } = string.Empty;
            public string vchEmail { get; set; } = string.Empty;
            public string vchAvatarUrl { get; set; } = string.Empty;
            public bool bCuentaVerificada { get; set; } = true;
        }
    }
}
