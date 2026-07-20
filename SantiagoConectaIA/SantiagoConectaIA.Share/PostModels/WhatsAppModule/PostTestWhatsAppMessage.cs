namespace SantiagoConectaIA.Share.PostModels.WhatsAppModule
{
	public class PostTestWhatsAppMessage
	{
		public string PhoneNumber { get; set; } = string.Empty;
		public string UserName { get; set; } = string.Empty;
		public string Message { get; set; } = string.Empty;

		public PostTestWhatsAppMessage()
		{
			PhoneNumber = string.Empty;
			UserName = string.Empty;
			Message = string.Empty;
		}
	}
}
