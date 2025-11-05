namespace SantiagoConectaIA.Share.PostModels.TramitesModule
{
	public class PostSaveRequisito
	{
		public int iIdRequisito { get; set; }
		public int iIdTramite { get; set; }
		public string vchNombre { get; set; }
		public string nvchDetalle { get; set; }
		public bool bObligatorio { get; set; }
		public bool bActivo { get; set; }
	}

}
