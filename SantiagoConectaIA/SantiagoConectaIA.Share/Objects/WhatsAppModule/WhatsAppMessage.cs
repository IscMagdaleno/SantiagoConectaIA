using System;

namespace SantiagoConectaIA.Share.Objects.WhatsAppModule
{
	public class WhatsAppMessage
	{
		public int iIdWhatsAppMessage { get; set; }
		public int iIdConversation { get; set; }
		public string nvchWhatsAppMessageId { get; set; }
		public string nvchDirection { get; set; }
		public string nvchMessageType { get; set; }
		public string nvchContent { get; set; }
		public DateTime dtTimestamp { get; set; }

		public WhatsAppMessage()
		{
			nvchWhatsAppMessageId = string.Empty;
			nvchDirection = string.Empty;
			nvchMessageType = "text";
			nvchContent = string.Empty;
			dtTimestamp = DateTime.Now;
		}
	}
}
