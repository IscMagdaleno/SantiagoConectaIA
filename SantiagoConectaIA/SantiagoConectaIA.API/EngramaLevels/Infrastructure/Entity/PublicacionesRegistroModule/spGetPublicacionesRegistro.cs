using EngramaCoreStandar.Dapper.Interfaces;

namespace SantiagoConectaIA.API.EngramaLevels.Infrastructure.Entity.PublicacionesRegistroModule
{
	public class spGetPublicacionesRegistro
	{
		public class Request : SpRequest
		{
			public string StoredProcedure => "spGetPublicacionesRegistro";
			public string vchTipo { get; set; }
			public bool? bExitoso { get; set; }
			public DateTime? dtDesde { get; set; }
			public DateTime? dtHasta { get; set; }
			public int iTop { get; set; }
		}

		public class Result : DbResult
		{
			public bool bResult { get; set; }
			public string vchMessage { get; set; }
			public int iIdPublicacionRegistro { get; set; }
			public string vchTipo { get; set; }
			public int iIdReferencia { get; set; }
			public string vchTitulo { get; set; }
			public string vchCanal { get; set; }
			public string vchOrigen { get; set; }
			public bool bExitoso { get; set; }
			public string vchResultado { get; set; }
			public string nvchMensaje { get; set; }
			public string vchImagenUrl { get; set; }
			public DateTime dtPublicacion { get; set; }
			public DateTime dtFechaLocal { get; set; }
		}
	}
}
