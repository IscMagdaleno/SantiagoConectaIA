using System;
using EngramaCoreStandar.Dapper.Interfaces;

namespace SantiagoConectaIA.API.EngramaLevels.Infrastructure.Entity.WhatsAppModule
{
	public class spGetWhatsAppDailyStats
	{
		public class Request : SpRequest
		{
			public string StoredProcedure => "spGetWhatsAppDailyStats";
			public int? iDays { get; set; }
		}

		public class Result : DbResult
		{
			public bool bResult { get; set; }
			public string vchMessage { get; set; }
			public DateTime? dtDate { get; set; }
			public int? iNewUsers { get; set; }
			public int? iMessagesInbound { get; set; }
			public int? iMessagesOutbound { get; set; }
			public int? iTotalMessages { get; set; }
			public int? iActiveConversations { get; set; }
		}
	}
}
