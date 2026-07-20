using System;
using EngramaCoreStandar.Dapper.Interfaces;

namespace SantiagoConectaIA.API.EngramaLevels.Infrastructure.Entity.WhatsAppModule
{
	public class spSaveWhatsAppUser
	{
		public class Request : SpRequest
		{
			public string StoredProcedure => "spSaveWhatsAppUser";
			public int? iIdWhatsAppUser { get; set; }
			public string nvchPhoneNumber { get; set; }
			public string nvchName { get; set; }
		}

		public class Result : DbResult
		{
			public bool bResult { get; set; }
			public string vchMessage { get; set; }
			public int? iIdWhatsAppUser { get; set; }
		}
	}
}
