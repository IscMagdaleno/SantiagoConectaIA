using EngramaCoreStandar.Dapper.Interfaces;
namespace SantiagoConectaIA.API.EngramaLevels.Infrastructure.Entity.ConversationalModule
{


	public class spSaveHistorialConsultaSimple
	{
		public class Request : SpRequest
		{
			public string StoredProcedure => "spSaveHistorialConsultaSimple";
			public int? iIdUsuario { get; set; }
			public string nvchPregunta { get; set; }
			public string nvchRespuesta { get; set; }
			public string vchOrigen { get; set; }
		}

		public class Result : DbResult
		{
			public bool bResult { get; set; }
			public string vchMessage { get; set; }
			public int iIdHistorial { get; set; }
		}
	}


}
