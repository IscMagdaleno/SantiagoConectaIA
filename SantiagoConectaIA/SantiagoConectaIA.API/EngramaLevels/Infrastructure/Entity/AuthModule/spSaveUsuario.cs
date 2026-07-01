
using EngramaCoreStandar.Dapper.Interfaces;

namespace SantiagoConectaIA.API.EngramaLevels.Infrastructure.Entity.AuthModule
{
    public class spSaveUsuario
    {
        public class Request : SpRequest
        {
            public string StoredProcedure => "spSaveUsuario";
            public int iIdUsuario { get; set; }
            public int iIdRol { get; set; }
            public string vchNombre { get; set; }
            public string vchUserName { get; set; }
            public string vchEmail { get; set; }
            public string vchPassword { get; set; }
            public bool bActivo { get; set; }
        }

        public class Result : DbResult
        {
            public bool bResult { get; set; }
            public string vchMessage { get; set; }
            public int iIdUsuario { get; set; }
        }
    }
}
