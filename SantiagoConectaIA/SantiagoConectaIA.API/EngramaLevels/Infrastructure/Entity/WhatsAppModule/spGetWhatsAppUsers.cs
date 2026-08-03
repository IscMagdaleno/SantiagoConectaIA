using System;
using EngramaCoreStandar.Dapper.Interfaces;

namespace SantiagoConectaIA.API.EngramaLevels.Infrastructure.Entity.WhatsAppModule
{
	public class spGetWhatsAppUsers
	{
		public class Request : SpRequest
		{
			public string StoredProcedure => "spGetWhatsAppUsers";
			public int? iTopRows { get; set; } = 100;
		}

		public class Result : DbResult
		{
			public bool bResult { get; set; }
			public string vchMessage { get; set; }
			public int? iIdWhatsAppUser { get; set; }
			public string nvchPhoneNumber { get; set; }
			public string nvchName { get; set; }
			public DateTime? dtFirstContact { get; set; }
			public DateTime? dtLastContact { get; set; }
			public int? iTotalMessages { get; set; }
			public bool? bActive { get; set; }
		}
	}
}
