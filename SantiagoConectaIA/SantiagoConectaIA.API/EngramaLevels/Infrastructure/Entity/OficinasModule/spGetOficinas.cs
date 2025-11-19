using EngramaCoreStandar.Dapper.Interfaces;

namespace SantiagoConectaIA.API.EngramaLevels.Infrastructure.Entity.OficinasModule
{
	// spGetOficinas.cs
	// Purpose: Request/Result para spGetOficinas (obtiene oficinas; admite ocultar contacto)
	public class spGetOficinas
	{
		public class Request : SpRequest
		{
			public string StoredProcedure => "spGetOficinas";
			public int iIdOficina { get; set; } = 0;
			public bool bActivo { get; set; } = false;
			public bool bIncluirContacto { get; set; } = false;
		}

		public class Result : DbResult
		{
			public bool bResult { get; set; }
			public string vchMessage { get; set; }
			public int iIdOficina { get; set; }
			public string vchNombre { get; set; }
			public string vchDireccion { get; set; }
			public string vchTelefono { get; set; } // puede ser null si bIncluirContacto = false
			public string vchEmail { get; set; } // puede ser null si bIncluirContacto = false
			public string vchHorario { get; set; }
			public double? flLatitud { get; set; }
			public double? flLongitud { get; set; }
			public string vchNotas { get; set; }
			public bool bActivo { get; set; }
			public DateTime? dtFechaCreacion { get; set; }
			public string vchUrlDireccion { get; set; }

		}
	}

}
