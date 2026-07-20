using System;

namespace SantiagoConectaIA.Share.Objects.WhatsAppModule
{
	public class WhatsAppUser
	{
		public int iIdWhatsAppUser { get; set; }
		public string nvchPhoneNumber { get; set; }
		public string nvchName { get; set; }
		public DateTime dtFirstContact { get; set; }
		public DateTime dtLastContact { get; set; }
		public int iTotalMessages { get; set; }
		public bool bActive { get; set; }

		public WhatsAppUser()
		{
			nvchPhoneNumber = string.Empty;
			nvchName = string.Empty;
			dtFirstContact = DateTime.Now;
			dtLastContact = DateTime.Now;
			iTotalMessages = 0;
			bActive = true;
		}
	}
}
