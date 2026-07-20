using System;
using EngramaCoreStandar.Dapper.Interfaces;

namespace SantiagoConectaIA.API.EngramaLevels.Infrastructure.Entity.WhatsAppModule
{
	public class spGetWhatsAppStats
	{
		public class Request : SpRequest
		{
			public string StoredProcedure => "spGetWhatsAppStats";
		}

		public class Result : DbResult
		{
			public bool bResult { get; set; }
			public string vchMessage { get; set; }
			public int? iTotalUsers { get; set; }
			public int? iTotalConversations { get; set; }
			public int? iTotalMessages { get; set; }
			public int? iActiveUsersToday { get; set; }
			public int? iActiveConversations { get; set; }
			public int? iMessagesToday { get; set; }
			public int? iNewUsersToday { get; set; }
			public double? flAvgMessagesPerConversation { get; set; }
		}
	}
}
