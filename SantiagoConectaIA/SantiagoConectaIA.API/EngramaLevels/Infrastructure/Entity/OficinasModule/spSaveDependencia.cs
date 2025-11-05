using EngramaCoreStandar.Dapper.Interfaces;

namespace SantiagoConectaIA.API.EngramaLevels.Infrastructure.Entity.OficinasModule
{
	// spSaveDependencia.cs
	// Purpose: Request/Result para spSaveDependencia (insert / update dependencia)
	public class spSaveDependencia
	{
		public class Request : SpRequest
		{
			public string StoredProcedure => "spSaveDependencia";
			public int iIdDependencia { get; set; } = 0;
			public string vchNombre { get; set; }
			public string nvchDescripcion { get; set; }
			public string vchUrlOficial { get; set; }
			public bool bActivo { get; set; } = true;
		}

		public class Result : DbResult
		{
			public bool bResult { get; set; }
			public string vchMessage { get; set; }
			public int iIdDependencia { get; set; }
		}
	}

}
