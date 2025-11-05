namespace SantiagoConectaIA.Share.Objects.OficinasModule
{
	public class OficinaPorTramite
	{
		public int iIdOficinaTramite { get; set; }
		public int iIdOficina { get; set; }
		public int iIdTramite { get; set; }
		public string vchNombre { get; set; }
		public string vchDireccion { get; set; }
		public string vchHorario { get; set; }
		public double? flLatitud { get; set; }
		public double? flLongitud { get; set; }
		public string vchTelefono { get; set; }
		public string vchEmail { get; set; }
		public string vchObservacion { get; set; }
		public bool bActivo { get; set; }
	}

}
