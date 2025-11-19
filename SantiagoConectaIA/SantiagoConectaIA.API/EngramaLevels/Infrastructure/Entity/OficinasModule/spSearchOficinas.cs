using EngramaCoreStandar.Dapper.Interfaces;

namespace SantiagoConectaIA.API.EngramaLevels.Infrastructure.Entity.OficinasModule
{
	// spSearchOficinas.cs
	// Purpose: Request/Result para spSearchOficinas (búsqueda paginada por texto y dependencia)
	public class spSearchOficinas
	{
		public class Request : SpRequest
		{
			public string StoredProcedure => "spSearchOficinas";
			public string vchTexto { get; set; }
			public int iPage { get; set; } = 1;
			public int iPageSize { get; set; } = 20;
			public bool bIncluirContacto { get; set; } = false;
		}

		public class Result : DbResult
		{
			public bool bResult { get; set; }
			public string vchMessage { get; set; }
			public int iIdOficina { get; set; }
			public string vchNombre { get; set; }
			public string vchDireccion { get; set; }
			public string vchHorario { get; set; }
			public double? flLatitud { get; set; }
			public double? flLongitud { get; set; }
			public string vchTelefono { get; set; } // null si no incluir
			public string vchEmail { get; set; } // null si no incluir
			public bool bActivo { get; set; }
			public DateTime? dtFechaCreacion { get; set; }
		}
	}

}
