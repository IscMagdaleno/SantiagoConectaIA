
using EngramaCoreStandar.Dapper.Interfaces;

namespace SantiagoConectaIA.API.EngramaLevels.Infrastructure.Entity.ConversationalModule
{
	public class spSearchTramitesForChat
	{
		public class Request : SpRequest
		{
			public string StoredProcedure => "spSearchTramitesForChat";
			// Parámetros de entrada
			public string vchTexto { get; set; }
			public int iLimit { get; set; }
		}

		public class Result : DbResult
		{
			public bool bResult { get; set; }
			public string vchMessage { get; set; }

			// Todos los campos de la tabla Tramite
			public int iIdTramite { get; set; }
			public string vchNombre { get; set; }
			public string nvchDescripcion { get; set; }
			public int iIdCategoria { get; set; }
			public bool bModalidadEnLinea { get; set; }
			public decimal mCosto { get; set; }
			public int? iIdOficina { get; set; } // Usamos int? ya que es NULLable en la base de datos
			public DateTime dtFechaCreacion { get; set; }
			public DateTime? dtFechaActualizacion { get; set; } // Usamos DateTime? ya que es NULLable en la base de datos
			public bool bActivo { get; set; }
		}
	}
}