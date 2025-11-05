
using EngramaCoreStandar.Dapper.Interfaces;

namespace SantiagoConectaIA.API.EngramaLevels.Infrastructure.Entity.ConversationalModule
{
	public class spSearchOficinasForChat
	{
		public class Request : SpRequest
		{
			public string StoredProcedure => "spSearchOficinasForChat";
			// Parámetros de entrada
			public string vchTexto { get; set; }
			public int iLimit { get; set; }
		}

		public class Result : DbResult
		{
			public bool bResult { get; set; }
			public string vchMessage { get; set; }

			// Todos los campos de la tabla Oficina
			public int iIdOficina { get; set; }
			public int? iIdDependencia { get; set; } // int? por ser NULLable
			public string vchNombre { get; set; }
			public string vchDireccion { get; set; }
			public string vchTelefono { get; set; }
			public string vchEmail { get; set; }
			public string vchHorario { get; set; }
			public double? flLatitud { get; set; } // double? por ser FLOAT NULLable
			public double? flLongitud { get; set; } // double? por ser FLOAT NULLable
			public string vchNotas { get; set; }
			public bool bActivo { get; set; }
			public DateTime dtFechaCreacion { get; set; }
		}
	}
}