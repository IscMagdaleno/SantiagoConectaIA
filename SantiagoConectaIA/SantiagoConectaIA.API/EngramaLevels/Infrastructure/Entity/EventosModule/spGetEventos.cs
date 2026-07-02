using EngramaCoreStandar.Dapper.Interfaces;
using EngramaCoreStandar.Dapper.Results;

namespace SantiagoConectaIA.API.EngramaLevels.Infrastructure.Entity.EventosModule
{
    public class spGetEventos
    {
        public class Request : SpRequest
        {
            public string StoredProcedure => "spGetEventos";
            public int iIdEvento { get; set; } = -1;
            public int? iIdCategoriaEvento { get; set; } = null;
            public bool? bEstatus { get; set; } = null;
            public bool? bDestacado { get; set; } = null;
        }

        public class Result : DbResult
        {
            public bool bResult { get; set; }
            public string vchMessage { get; set; }
            public int iIdEvento { get; set; }
            public int? iIdCategoriaEvento { get; set; }
            public string vchCategoriaNombre { get; set; }
            public string vchNombre { get; set; }
            public string nvchDescripcion { get; set; }
            public System.DateTime dtFechaInicio { get; set; }
            public System.DateTime? dtFechaFin { get; set; }
            public string vchLugar { get; set; }
            public string vchDireccion { get; set; }
            public double flLatitud { get; set; }
            public double flLongitud { get; set; }
            public string vchImagenPortada { get; set; }
            public string vchCostoTexto { get; set; }
            public string vchOrganizador { get; set; }
            public string vchTelefono { get; set; }
            public string vchCorreo { get; set; }
            public string vchUrlOficial { get; set; }
            public bool bDestacado { get; set; }
            public bool bEstatus { get; set; }
            public System.DateTime dtFechaRegistro { get; set; }
        }
    }
}
