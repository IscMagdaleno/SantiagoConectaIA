using EngramaCoreStandar.Dapper.Interfaces;

namespace SantiagoConectaIA.API.EngramaLevels.Infrastructure.Entity.TramitesModule
{
	public class spGetRequisitosPorTramite
	{
		public class Request : SpRequest
		{
			public string StoredProcedure => "spGetRequisitosPorTramite";
			public int iIdTramite { get; set; }
		}

		public class Result : DbResult
		{
			public bool bResult { get; set; }
			public string vchMessage { get; set; }
			public int iIdRequisito { get; set; }
			public int iIdTramite { get; set; }
			public string vchNombre { get; set; }
			public string nvchDetalle { get; set; }
			public bool bObligatorio { get; set; }
			public bool bActivo { get; set; }
		}
	}

}
