using EngramaCoreStandar.Dapper.Interfaces;

namespace SantiagoConectaIA.API.EngramaLevels.Infrastructure.Entity.TramitesModule
{
	public class spSaveRequisito
	{
		public class Request : SpRequest
		{
			public string StoredProcedure => "spSaveRequisito";
			public int iIdRequisito { get; set; } = 0;
			public int iIdTramite { get; set; }
			public string vchNombre { get; set; }
			public string nvchDetalle { get; set; }
			public bool bObligatorio { get; set; } = true;
			public bool bActivo { get; set; } = true;
		}

		public class Result : DbResult
		{
			public bool bResult { get; set; }
			public string vchMessage { get; set; }
			public int iIdRequisito { get; set; }
		}
	}

}
