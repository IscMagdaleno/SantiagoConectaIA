namespace SantiagoConectaIA.Share.PostModels.TramitesModule
{
	public class PostSaveDocumento
	{
		public int iIdDocumento { get; set; }
		public int iIdTramite { get; set; }
		public string vchNombre { get; set; }
		public string vchUrlDocumento { get; set; }
		public bool bActivo { get; set; }
	}

}
