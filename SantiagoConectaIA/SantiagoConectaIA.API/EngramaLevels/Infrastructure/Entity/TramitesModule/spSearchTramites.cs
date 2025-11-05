using EngramaCoreStandar.Dapper.Interfaces;

namespace SantiagoConectaIA.API.EngramaLevels.Infrastructure.Entity.TramitesModule
{
	public class spSearchTramites
	{
		public class Request : SpRequest
		{
			public string StoredProcedure => "spSearchTramites";
			public string vchTexto { get; set; }
			public int iIdCategoria { get; set; } = 0;
			public int iPage { get; set; } = 1;
			public int iPageSize { get; set; } = 20;
		}

		public class Result : DbResult
		{
			public bool bResult { get; set; }
			public string vchMessage { get; set; }
			public int iIdTramite { get; set; }
			public string vchNombre { get; set; }
			public string nvchDescripcion { get; set; }
			public int iIdCategoria { get; set; }
			public bool bModalidadEnLinea { get; set; }
			public decimal mCosto { get; set; }
			public DateTime? dtFechaCreacion { get; set; }
		}
	}

}
