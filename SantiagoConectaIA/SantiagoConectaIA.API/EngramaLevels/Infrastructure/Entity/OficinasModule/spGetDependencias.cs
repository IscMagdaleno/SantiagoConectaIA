using EngramaCoreStandar.Dapper.Interfaces;

namespace SantiagoConectaIA.API.EngramaLevels.Infrastructure.Entity.OficinasModule
{
	// spGetDependencias.cs
	// Purpose: Request/Result para spGetDependencias (obtiene dependencias)
	public class spGetDependencias
	{
		public class Request : SpRequest
		{
			public string StoredProcedure => "spGetDependencias";
			public int iIdDependencia { get; set; } = 0;
			public bool bActivo { get; set; } = false;
		}

		public class Result : DbResult
		{
			public bool bResult { get; set; }
			public string vchMessage { get; set; }
			public int iIdDependencia { get; set; }
			public string vchNombre { get; set; }
			public string nvchDescripcion { get; set; }
			public string vchUrlOficial { get; set; }
			public bool bActivo { get; set; }
			public DateTime? dtFechaCreacion { get; set; }
		}
	}

}
