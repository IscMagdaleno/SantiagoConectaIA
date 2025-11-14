namespace SantiagoConectaIA.Share.Objects.OficinasModule
{
	public class Oficina
	{
		public int iIdOficina { get; set; }
		public int? iIdDependencia { get; set; }
		public string vchNombre { get; set; }
		public string vchDireccion { get; set; }
		public string vchTelefono { get; set; }
		public string vchEmail { get; set; }
		public string vchHorario { get; set; }
		public double? flLatitud { get; set; }
		public double? flLongitud { get; set; }
		public string vchNotas { get; set; }
		public bool bActivo { get; set; }
		public string vchUrlDireccion { get; set; }

		public Oficina (){
  			vchNombre = string.Empty;
			vchDireccion = string.Empty;
			vchTelefono = string.Empty;
			vchEmail = string.Empty;
			vchHorario = string.Empty;
			vchNotas = string.Empty;
			vchUrlDireccion = string.Empty;
		}
	}

}
