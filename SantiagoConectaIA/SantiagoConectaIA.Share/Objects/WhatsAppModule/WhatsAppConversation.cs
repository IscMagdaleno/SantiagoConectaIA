using System;

namespace SantiagoConectaIA.Share.Objects.WhatsAppModule
{
	public class WhatsAppConversation
	{
		public int iIdConversation { get; set; }
		public int iIdWhatsAppUser { get; set; }
		public DateTime dtStartTime { get; set; }
		public DateTime? dtEndTime { get; set; }
		public int iMessageCount { get; set; }
		public string nvchStatus { get; set; }

		public WhatsAppConversation()
		{
			nvchStatus = "active";
			dtStartTime = DateTime.Now;
			iMessageCount = 0;
		}
	}
}
