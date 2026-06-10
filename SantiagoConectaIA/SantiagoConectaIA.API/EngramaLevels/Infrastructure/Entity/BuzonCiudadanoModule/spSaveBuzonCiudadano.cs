using System;
using EngramaCoreStandar.Dapper.Interfaces;

namespace SantiagoConectaIA.API.EngramaLevels.Infrastructure.Entity.BuzonCiudadanoModule
{
	public class spSaveBuzonCiudadano
	{
		public class Request : SpRequest
		{
			public string StoredProcedure => "spSaveBuzonCiudadano";
			public string nvchNombreCiudadano { get; set; } = string.Empty;
			public string? nvchEmail { get; set; }
			public string? nvchTelefono { get; set; }
			public string nvchCategoria { get; set; } = string.Empty;
			public string nvchDescripcion { get; set; } = string.Empty;
			public string? nvchThreadId { get; set; }
		}

		public class Result : DbResult
		{
			public bool bResult { get; set; }
			public string vchMessage { get; set; } = string.Empty;
			public int? iIdReporte { get; set; }
		}
	}
}
