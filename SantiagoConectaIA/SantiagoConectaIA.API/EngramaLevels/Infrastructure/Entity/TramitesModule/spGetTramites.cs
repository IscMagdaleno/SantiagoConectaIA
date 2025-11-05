using EngramaCoreStandar.Dapper.Interfaces;

namespace SantiagoConectaIA.API.EngramaLevels.Infrastructure.Entity.TramitesModule
{
	public class spGetTramites
	{
		public class Request : SpRequest
		{
			public string StoredProcedure => "spGetTramites";
			public int iIdTramite { get; set; } = 0;
			public bool bActivo { get; set; } = false;
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
			public int iIdOficina { get; set; }
			public DateTime? dtFechaCreacion { get; set; }
			public DateTime? dtFechaActualizacion { get; set; }
			public bool bActivo { get; set; }
		}
	}

}
