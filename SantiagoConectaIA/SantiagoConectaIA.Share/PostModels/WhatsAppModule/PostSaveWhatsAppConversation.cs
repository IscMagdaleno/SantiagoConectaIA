using System;

namespace SantiagoConectaIA.Share.PostModels.WhatsAppModule
{
	public class PostSaveWhatsAppConversation
	{
		public int iIdConversation { get; set; }
		public int iIdWhatsAppUser { get; set; }
		public string nvchStatus { get; set; }

		public PostSaveWhatsAppConversation()
		{
			nvchStatus = "active";
		}
	}
}
