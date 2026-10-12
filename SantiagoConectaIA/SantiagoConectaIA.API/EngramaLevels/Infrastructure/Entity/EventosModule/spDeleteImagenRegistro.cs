using EngramaCoreStandar.Dapper.Interfaces;

namespace SantiagoConectaIA.API.EngramaLevels.Infrastructure.Entity.EventosModule
{
	public class spDeleteImagenRegistro
	{
		public class Request : SpRequest
		{
			public string StoredProcedure => "spDeleteImagenRegistro";
			public int iIdImagen { get; set; }
		}

		public class Result : DbResult
		{
			public bool bResult { get; set; }
			public string vchMessage { get; set; }
		}
	}
}
