namespace SantiagoConectaIA.Share.PostModels.BuzonCiudadanoModule
{
	public class PostSaveBuzonCiudadano
	{
		public string nvchNombreCiudadano { get; set; } = string.Empty;
		public string? nvchEmail { get; set; }
		public string? nvchTelefono { get; set; }
		public string nvchCategoria { get; set; } = string.Empty;
		public string nvchDescripcion { get; set; } = string.Empty;
		public string? nvchThreadId { get; set; }
	}
}
