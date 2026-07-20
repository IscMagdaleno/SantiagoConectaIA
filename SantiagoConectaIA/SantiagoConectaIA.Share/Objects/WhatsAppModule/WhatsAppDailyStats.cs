using System;

namespace SantiagoConectaIA.Share.Objects.WhatsAppModule
{
	public class WhatsAppDailyStats
	{
		public DateTime dtDate { get; set; }
		public int iNewUsers { get; set; }
		public int iMessagesInbound { get; set; }
		public int iMessagesOutbound { get; set; }
		public int iTotalMessages { get; set; }
		public int iActiveConversations { get; set; }
	}
}
