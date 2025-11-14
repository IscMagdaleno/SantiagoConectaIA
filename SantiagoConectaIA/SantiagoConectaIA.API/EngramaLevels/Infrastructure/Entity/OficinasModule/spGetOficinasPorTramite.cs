using EngramaCoreStandar.Dapper.Interfaces;

namespace SantiagoConectaIA.API.EngramaLevels.Infrastructure.Entity.OficinasModule
{
	// spGetOficinasPorTramite.cs
	// Purpose: Request/Result para spGetOficinasPorTramite (obtiene oficinas asociadas a un trámite)
	public class spGetOficinasPorTramite
	{
		public class Request : SpRequest
		{
			public string StoredProcedure => "spGetOficinasPorTramite";
			public int iIdTramite { get; set; }
			public string vchTexto { get; set; } // filtro opcional
			public bool bIncluirContacto { get; set; } = false;
		}

		public class Result : DbResult
		{
			public bool bResult { get; set; }
			public string vchMessage { get; set; }
			public int iIdOficinaTramite { get; set; }
			public int iIdOficina { get; set; }
			public int iIdTramite { get; set; }
			public string vchNombre { get; set; }
			public string vchDireccion { get; set; }
			public string vchHorario { get; set; }
			public double? flLatitud { get; set; }
			public double? flLongitud { get; set; }
			public string vchTelefono { get; set; } // null si no incluir
			public string vchEmail { get; set; } // null si no incluir
			public string vchObservacion { get; set; }
			public bool bActivo { get; set; }
			public DateTime? dtFechaCreacion { get; set; }
			public string vchUrlDireccion { get; set; }
		}
	}

}
