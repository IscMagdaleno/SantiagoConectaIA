using EngramaCoreStandar.Dapper.Interfaces;
using EngramaCoreStandar.Dapper.Results;
using System;

namespace SantiagoConectaIA.API.EngramaLevels.Infrastructure.Entity.EmpresasModule
{
    public class spGetPropietario
    {
        public class Request : SpRequest
        {
            public string StoredProcedure => "spGetPropietario";

            public int iIdPropietario { get; set; }
            public string vchCorreo { get; set; }
            public bool bActivo { get; set; }
        }

        public class Result : DbResult
        {
            public bool bResult { get; set; }
            public string vchMessage { get; set; }
            public int iIdPropietario { get; set; }
            public string vchNombre { get; set; }
            public string vchCorreo { get; set; }
            public string vchTelefono { get; set; }
            public DateTime dtFechaCreacion { get; set; }
            public bool bActivo { get; set; }
        }
    }
}
