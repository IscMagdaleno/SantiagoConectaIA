using EngramaCoreStandar.Dapper.Interfaces;

namespace SantiagoConectaIA.API.EngramaLevels.Infrastructure.Entity.TramitesModule
{
	public class spSaveTramite
	{
		public class Request : SpRequest
		{
			public string StoredProcedure => "spSaveTramite";
			public int iIdTramite { get; set; } = 0;
			public string vchNombre { get; set; }
			public string nvchDescripcion { get; set; }
			public int iIdCategoria { get; set; } = 0;
			public bool bModalidadEnLinea { get; set; } = false;
			public decimal mCosto { get; set; } = 0;
			public int? iIdOficina { get; set; } = null;
			public bool bActivo { get; set; } = true;
		}

		public class Result : DbResult
		{
			public bool bResult { get; set; }
			public string vchMessage { get; set; }
			public int iIdTramite { get; set; }
		}
	}

}
