namespace SantiagoConectaIA.Share.PostModels.PublicacionesModule
{
	public class PostSavePublicacionRegistro
	{
		public int iIdPublicacionRegistro { get; set; }
		public string vchTipo { get; set; } = string.Empty;
		public int iIdReferencia { get; set; }
		public string? vchTitulo { get; set; }
		public string vchCanal { get; set; } = "FACEBOOK";
		public string vchOrigen { get; set; } = "AUTOMATICA";
		public bool bExitoso { get; set; }
		public string? vchResultado { get; set; }
		public string? nvchMensaje { get; set; }
		public string? vchImagenUrl { get; set; }

		/// <summary>UTC. Si es nulo se usa la fecha actual.</summary>
		public DateTime? dtPublicacion { get; set; }
	}
}
