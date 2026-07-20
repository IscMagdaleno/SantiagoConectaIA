namespace SantiagoConectaIA.Share.Objects.WhatsAppModule
{
	public class WhatsAppStats
	{
		public int iTotalUsers { get; set; }
		public int iTotalConversations { get; set; }
		public int iTotalMessages { get; set; }
		public int iActiveUsersToday { get; set; }
		public int iActiveConversations { get; set; }
		public int iMessagesToday { get; set; }
		public int iNewUsersToday { get; set; }
		public double flAvgMessagesPerConversation { get; set; }
	}
}
