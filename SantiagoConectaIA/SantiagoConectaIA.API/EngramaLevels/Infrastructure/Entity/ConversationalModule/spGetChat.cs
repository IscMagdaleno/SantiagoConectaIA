using System;
using EngramaCoreStandar.Dapper.Interfaces;

namespace SantiagoConectaIA.API.EngramaLevels.Infrastructure.Entity.ConversationalModule
{
	public class spGetChat
	{
		public class Request : SpRequest
		{
			public string StoredProcedure => "spGetChat";
			public int? iIdProyecto { get; set; }
		}

		public class Result : DbResult
		{
			public bool bResult { get; set; }
			public string vchMessage { get; set; }
			public int? iIdChat { get; set; }
			public int? iIdProyecto { get; set; }
			public DateTime? dtFechaCreacion { get; set; }
			public string nvchNombre { get; set; }
			public bool? bActivo { get; set; }
			public string nvchThreadId { get; set; }
		}
	}
}
