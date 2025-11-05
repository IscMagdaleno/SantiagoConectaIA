using EngramaCoreStandar.Dapper.Interfaces;

namespace SantiagoConectaIA.API.EngramaLevels.Infrastructure.Entity.ConversationalModule
{


	public class spSearchRequisitosForChat
	{
		public class Request : SpRequest
		{
			public string StoredProcedure => "spSearchRequisitosForChat";
			public int iIdTramite { get; set; }
		}

		public class Result : DbResult
		{
			public bool bResult { get; set; }
			public string vchMessage { get; set; }
			// Propiedades de salida del SP
			public string vchTipo { get; set; }
			public string vchNombre { get; set; }
			public string nvchDetalle { get; set; }
			public bool bObligatorio { get; set; }
		}
	}

}
