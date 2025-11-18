using EngramaCoreStandar.Dapper.Interfaces;

namespace SantiagoConectaIA.API.EngramaLevels.Infrastructure.Entity.TramitesModule
{
	public class spGetDocumentosPorTramite
	{
		public class Request : SpRequest
		{
			public string StoredProcedure => "spGetDocumentosPorTramite";
			public int iIdTramite { get; set; }
		}

		public class Result : DbResult
		{
			public bool bResult { get; set; }
			public string vchMessage { get; set; }
			public int iIdDocumento { get; set; }
			public string vchNombre { get; set; }
			public string vchUrl { get; set; }
		}
	}
}
