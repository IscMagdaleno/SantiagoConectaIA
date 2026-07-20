using System;

namespace SantiagoConectaIA.Share.PostModels.WhatsAppModule
{
	public class PostSaveWhatsAppUser
	{
		public int iIdWhatsAppUser { get; set; }
		public string nvchPhoneNumber { get; set; }
		public string nvchName { get; set; }

		public PostSaveWhatsAppUser()
		{
			nvchPhoneNumber = string.Empty;
			nvchName = string.Empty;
		}
	}
}
