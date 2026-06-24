namespace SantiagoConectaIA.Share.PostModels.TramitesModule
{
	public class PostSaveTramite
	{
		public int iIdTramite { get; set; }
		public string vchNombre { get; set; }
		public string nvchDescripcion { get; set; }
		public string vchNombreEn { get; set; }
		public string nvchDescripcionEn { get; set; }
		public int iIdCategoria { get; set; }
		public bool bModalidadEnLinea { get; set; }
		public decimal mCosto { get; set; }
		public bool bPrecioCalculado { get; set; }
		public int iIdOficina { get; set; }
		public bool bActivo { get; set; }

		public List<PostSaveRequisito> Requisitos { get; set; } = new List<PostSaveRequisito>();
		public List<PostSaveTramitePaso> Pasos { get; set; } = new List<PostSaveTramitePaso>();
		public List<PostSaveDocumento> Documentos { get; set; } = new List<PostSaveDocumento>();
	}

	public class PostSaveTramitePaso
	{
		public short iOrden { get; set; }
		public string nvchDescripcion { get; set; }
	}
}
