
using EngramaCoreStandar.Dapper.Interfaces;


namespace SantiagoConectaIA.API.EngramaLevels.Infrastructure.Entity.ConversationalModule
{
	public class spSearchOficinasByTramite
	{
		public class Request : SpRequest
		{
			public string StoredProcedure => "spSearchOficinasByTramite";
			// El nombre del parámetro debe coincidir con el SP
			public int iIdTramite { get; set; }
		}

		// El resultado es el mismo que el resultado completo de una Oficina
		public class Result : spSearchOficinasForChat.Result { }
	}
}