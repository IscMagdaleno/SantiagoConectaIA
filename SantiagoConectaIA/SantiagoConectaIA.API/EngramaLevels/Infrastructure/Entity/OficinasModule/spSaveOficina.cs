using EngramaCoreStandar.Dapper.Interfaces;

namespace SantiagoConectaIA.API.EngramaLevels.Infrastructure.Entity.OficinasModule
{
	// spSaveOficina.cs
	// Purpose: Request/Result para spSaveOficina (insert / update oficina)
	public class spSaveOficina
	{
		public class Request : SpRequest
		{
			public string StoredProcedure => "spSaveOficina";
			public int iIdOficina { get; set; } = 0;
			public int? iIdDependencia { get; set; } = null;
			public string vchNombre { get; set; }
			public string vchDireccion { get; set; }
			public string vchTelefono { get; set; }
			public string vchEmail { get; set; }
			public string vchHorario { get; set; }
			public double? flLatitud { get; set; }
			public double? flLongitud { get; set; }
			public string vchNotas { get; set; }
			public bool bActivo { get; set; } = true;
		}

		public class Result : DbResult
		{
			public bool bResult { get; set; }
			public string vchMessage { get; set; }
			public int iIdOficina { get; set; }
		}
	}

}
