using System;
using EngramaCoreStandar.Dapper.Interfaces;

namespace SantiagoConectaIA.API.EngramaLevels.Infrastructure.Entity.WhatsAppModule
{
	public class spSaveWhatsAppMessage
	{
		public class Request : SpRequest
		{
			public string StoredProcedure => "spSaveWhatsAppMessage";
			public int? iIdConversation { get; set; }
			public string nvchWhatsAppMessageId { get; set; }
			public string nvchDirection { get; set; }
			public string nvchMessageType { get; set; }
			public string nvchContent { get; set; }
			public DateTime? dtTimestamp { get; set; }
		}

		public class Result : DbResult
		{
			public bool bResult { get; set; }
			public string vchMessage { get; set; }
			public int? iIdWhatsAppMessage { get; set; }
		}
	}
}
