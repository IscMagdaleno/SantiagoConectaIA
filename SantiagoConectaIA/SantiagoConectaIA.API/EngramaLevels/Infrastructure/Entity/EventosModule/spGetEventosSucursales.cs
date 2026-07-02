using EngramaCoreStandar.Dapper.Interfaces;
using EngramaCoreStandar.Dapper.Results;

namespace SantiagoConectaIA.API.EngramaLevels.Infrastructure.Entity.EventosModule
{
    public class spGetEventosSucursales
    {
        public class Request : SpRequest
        {
            public string StoredProcedure => "spGetEventosSucursales";
            public int iIdSucursalEvento { get; set; } = -1;
            public int? iIdEvento { get; set; } = null;
            public bool? bActivo { get; set; } = null;
        }

        public class Result : DbResult
        {
            public bool bResult { get; set; }
            public string vchMessage { get; set; }
            public int iIdSucursalEvento { get; set; }
            public int iIdEvento { get; set; }
            public string vchNombre { get; set; }
            public string vchDireccion { get; set; }
            public double flLatitud { get; set; }
            public double flLongitud { get; set; }
            public string vchTelefono { get; set; }
            public string vchContacto { get; set; }
            public string vchHorario { get; set; }
            public string vchNotas { get; set; }
            public bool bActivo { get; set; }
            public System.DateTime dtFechaRegistro { get; set; }
        }
    }
}
