namespace SantiagoConectaIA.Share.PostModels.TramitesModule
{
	public class PostSaveTramite
	{
		public int iIdTramite { get; set; }
		public string vchNombre { get; set; }
		public string nvchDescripcion { get; set; }
		public int iIdCategoria { get; set; }
		public bool bModalidadEnLinea { get; set; }
		public decimal mCosto { get; set; }
		public int iIdOficina { get; set; }
		public bool bActivo { get; set; }
	}

}
