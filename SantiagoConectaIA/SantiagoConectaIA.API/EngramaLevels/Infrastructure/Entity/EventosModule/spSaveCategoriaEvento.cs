using EngramaCoreStandar.Dapper.Interfaces;

namespace SantiagoConectaIA.API.EngramaLevels.Infrastructure.Entity.EventosModule
{
	public class spSaveCategoriaEvento
	{
		public class Request : SpRequest
		{
			public string StoredProcedure => "spSaveCategoriaEvento";
			public int iIdCategoriaEvento { get; set; }
			public string vchNombre { get; set; }
			public string vchIconoUrl { get; set; }
		}

		public class Result : DbResult
		{
			public bool bResult { get; set; }
			public string vchMessage { get; set; }
			public int iIdCategoriaEvento { get; set; }
		}
	}
}
