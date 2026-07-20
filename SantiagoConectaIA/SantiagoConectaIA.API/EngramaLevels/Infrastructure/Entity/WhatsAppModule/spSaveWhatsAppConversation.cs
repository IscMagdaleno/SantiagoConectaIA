using System;
using EngramaCoreStandar.Dapper.Interfaces;

namespace SantiagoConectaIA.API.EngramaLevels.Infrastructure.Entity.WhatsAppModule
{
	public class spSaveWhatsAppConversation
	{
		public class Request : SpRequest
		{
			public string StoredProcedure => "spSaveWhatsAppConversation";
			public int? iIdConversation { get; set; }
			public int? iIdWhatsAppUser { get; set; }
			public string nvchStatus { get; set; }
		}

		public class Result : DbResult
		{
			public bool bResult { get; set; }
			public string vchMessage { get; set; }
			public int? iIdConversation { get; set; }
		}
	}
}
