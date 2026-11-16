using EngramaCoreStandar.Dapper.Interfaces;

namespace SantiagoConectaIA.API.EngramaLevels.Infrastructure.Entity.MemesModule
{
	public class spDescartarMeme
	{
		public class Request : SpRequest
		{
			public string StoredProcedure => "spDescartarMeme";
			public int iIdMeme { get; set; }
		}

		public class Result : DbResult
		{
			public bool bResult { get; set; }
			public string vchMessage { get; set; }
		}
	}
}
