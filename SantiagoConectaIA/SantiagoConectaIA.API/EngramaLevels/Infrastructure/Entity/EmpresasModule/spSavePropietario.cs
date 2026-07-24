using EngramaCoreStandar.Dapper.Interfaces;
using EngramaCoreStandar.Dapper.Results;
using System;

namespace SantiagoConectaIA.API.EngramaLevels.Infrastructure.Entity.EmpresasModule
{
    public class spSavePropietario
    {
        public class Request : SpRequest
        {
            public string StoredProcedure => "spSavePropietario";

            public int iIdPropietario { get; set; }
            public string vchNombre { get; set; }
            public string vchCorreo { get; set; }
            public string vchTelefono { get; set; }
            public bool bActivo { get; set; }
        }

        public class Result : DbResult
        {
            public bool bResult { get; set; }
            public string vchMessage { get; set; }
            public int iIdPropietario { get; set; }
        }
    }
}
