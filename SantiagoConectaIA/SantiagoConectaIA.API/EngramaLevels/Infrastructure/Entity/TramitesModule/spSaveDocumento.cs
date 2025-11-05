using EngramaCoreStandar.Dapper.Interfaces;

namespace SantiagoConectaIA.API.EngramaLevels.Infrastructure.Entity.TramitesModule
{
	public class spSaveDocumento
	{
		public class Request : SpRequest
		{
			public string StoredProcedure => "spSaveDocumento";
			public int iIdDocumento { get; set; } = 0;
			public int iIdTramite { get; set; }
			public string vchNombre { get; set; }
			public string vchUrl { get; set; }
			public bool bActivo { get; set; } = true;
		}

		public class Result : DbResult
		{
			public bool bResult { get; set; }
			public string vchMessage { get; set; }
			public int iIdDocumento { get; set; }
		}
	}

}
