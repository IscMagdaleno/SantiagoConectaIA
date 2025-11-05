
using EngramaCoreStandar.Dapper.Interfaces;

namespace SantiagoConectaIA.API.EngramaLevels.Infrastructure.Entity.ConversationalModule
{
	public class spSearchCostoForChat
	{
		public class Request : SpRequest
		{
			public string StoredProcedure => "spSearchCostoForChat";
			// El nombre del parámetro debe coincidir con el SP
			public int iIdTramite { get; set; }
		}

		public class Result : DbResult
		{
			public bool bResult { get; set; }
			public string vchMessage { get; set; }

			// Propiedades de salida del SP
			public decimal mCosto { get; set; }
			public bool bModalidadEnLinea { get; set; }
		}
	}
}