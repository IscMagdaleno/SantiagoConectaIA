using EngramaCoreStandar.Dapper.Interfaces;

namespace SantiagoConectaIA.API.EngramaLevels.Infrastructure.Entity.OficinasModule
{
	// spLinkOficinaTramite.cs
	// Purpose: Request/Result para spLinkOficinaTramite (crear/actualizar relación Oficina <-> Tramite)
	public class spLinkOficinaTramite
	{
		public class Request : SpRequest
		{
			public string StoredProcedure => "spLinkOficinaTramite";
			public int iIdOficina { get; set; }
			public int iIdTramite { get; set; }
			public string vchObservacion { get; set; }
			public bool bActivo { get; set; } = true;
		}

		public class Result : DbResult
		{
			public bool bResult { get; set; }
			public string vchMessage { get; set; }
			public int iIdOficinaTramite { get; set; }
		}
	}

}
