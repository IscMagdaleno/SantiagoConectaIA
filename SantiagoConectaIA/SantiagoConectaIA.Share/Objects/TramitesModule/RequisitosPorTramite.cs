namespace SantiagoConectaIA.Share.Objects.TramitesModule
{
	public class RequisitosPorTramite
	{
		public int iIdRequisito { get; set; }
		public int iIdTramite { get; set; }
		public string vchNombre { get; set; }
		public string nvchDetalle { get; set; }
		public bool bObligatorio { get; set; }
		public bool bActivo { get; set; }
		public RequisitosPorTramite()
		{
			vchNombre = string.Empty;
			nvchDetalle = string.Empty;
		}

	}
}
