using EngramaCoreStandar.Dapper.Interfaces;
using System;

namespace SantiagoConectaIA.API.EngramaLevels.Infrastructure.Entity.AuthModule
{
    public class spGetUsuarioAuth
    {
        public class Request : SpRequest
        {
            public string StoredProcedure => "spGetUsuarioAuth";
            public string vchUserOrEmail { get; set; }
            public string vchPassword { get; set; }
        }

        public class Result : DbResult
        {
            public bool bResult { get; set; }
            public string vchMessage { get; set; }
            public int iIdUsuario { get; set; }
            public string vchNombre { get; set; }
            public string vchUserName { get; set; }
            public string vchEmail { get; set; }
            public string vchRol { get; set; }
        }
    }
}
