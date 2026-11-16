using EngramaCoreStandar.Dapper.Interfaces;

namespace SantiagoConectaIA.API.EngramaLevels.Infrastructure.Entity.MemesModule
{
	public class spGetMemesResumen
	{
		public class Request : SpRequest
		{
			public string StoredProcedure => "spGetMemesResumen";
		}

		public class Result : DbResult
		{
			public bool bResult { get; set; }
			public string vchMessage { get; set; }
			public int iUsoHoy { get; set; }
		}
	}
}
