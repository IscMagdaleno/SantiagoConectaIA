namespace SantiagoConectaIA.Share.Objects.TramitesModule
{
	public class Pasos
	{
		public int iIdTramitePaso { get; set; }
		public int iIdTramite { get; set; }
		public short iOrden { get; set; }
		public string nvchDescripcion { get; set; }
		public bool bActivo { get; set; }

		public Pasos()
		{
			nvchDescripcion = string.Empty;
		}
	}
}
