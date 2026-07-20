using System;

namespace SantiagoConectaIA.Share.PostModels.WhatsAppModule
{
	public class PostSaveWhatsAppMessage
	{
		public int iIdConversation { get; set; }
		public string nvchWhatsAppMessageId { get; set; }
		public string nvchDirection { get; set; }
		public string nvchMessageType { get; set; }
		public string nvchContent { get; set; }
		public DateTime dtTimestamp { get; set; }

		public PostSaveWhatsAppMessage()
		{
			nvchWhatsAppMessageId = string.Empty;
			nvchDirection = "inbound";
			nvchMessageType = "text";
			nvchContent = string.Empty;
			dtTimestamp = DateTime.Now;
		}
	}
}
