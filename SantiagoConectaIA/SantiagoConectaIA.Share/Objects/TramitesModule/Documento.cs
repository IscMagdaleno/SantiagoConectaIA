namespace SantiagoConectaIA.Share.Objects.TramitesModule
{
	public class Documento
	{
		public int iIdDocumento { get; set; }
		public int iIdTramite { get; set; }
		public string vchNombre { get; set; }
		public string vchUrlDocumento { get; set; }
		public bool bActivo { get; set; }
		public Documento()
		{
			vchNombre = string.Empty;
			vchUrlDocumento = string.Empty;
			bActivo = true;
		}

	}
}
