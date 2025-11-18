using EngramaCoreStandar.Dapper.Interfaces;

namespace SantiagoConectaIA.API.EngramaLevels.Infrastructure.Entity.TramitesModule
{
	public class spGetPasosPorTramite
	{
		public class Request : SpRequest
		{
			public string StoredProcedure => "spGetPasosPorTramite";
			public int iIdTramite { get; set; }
		}

		public class Result : DbResult
		{
			public bool bResult { get; set; }
			public string vchMessage { get; set; }
			public int iIdTramitePaso { get; set; }
			public short iOrden { get; set; }
			public string nvchDescripcion { get; set; }
		}
	}
}
