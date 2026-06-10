using System;

namespace SantiagoConectaIA.Share.Objects.BuzonCiudadanoModule
{
	public class BuzonCiudadano
	{
		public int iIdReporte { get; set; }
		public string nvchNombreCiudadano { get; set; } = string.Empty;
		public string? nvchEmail { get; set; }
		public string? nvchTelefono { get; set; }
		public string nvchCategoria { get; set; } = string.Empty;
		public string nvchDescripcion { get; set; } = string.Empty;
		public DateTime dtFechaReporte { get; set; }
		public string? nvchThreadId { get; set; }
	}
}
