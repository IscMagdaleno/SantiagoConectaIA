using EngramaCoreStandar.Dapper.Interfaces;

namespace SantiagoConectaIA.API.EngramaLevels.Infrastructure.Entity.TramitesModule
{
	public class spGetTramitesCard
	{
		public class Request : SpRequest
		{
			public string StoredProcedure => "spGetTramitesCard";
			public int iIdTramite { get; set; }
			public bool bActivo { get; set; }
		}

		public class Result : DbResult
		{
			public bool bResult { get; set; }
			public string vchMessage { get; set; }

			// Campos de datos
			public int iIdTramite { get; set; }
			public int iIdOficina { get; set; }
			public string vchNombreTramite { get; set; }
			public string vchDescripcionTramite { get; set; }
			public string vchNombreOficina { get; set; }
			public string vchDireccionOficina { get; set; }
			public string vchTelefonoOficina { get; set; }
			public bool bModalidadEnLinea { get; set; }
			public decimal dCosto { get; set; }
		}
	}
}
