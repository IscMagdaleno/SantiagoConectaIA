using SantiagoConectaIA.Share.Objects.OficinasModule;

using System.ComponentModel.DataAnnotations;

namespace SantiagoConectaIA.Share.Objects.TramitesModule
{
	public class Tramite
	{
		public int iIdTramite { get; set; }


		[Required]
		public string vchNombre { get; set; }

		[Required]
		public string nvchDescripcion { get; set; }

		public int iIdCategoria { get; set; }
		public bool bModalidadEnLinea { get; set; }
		public decimal mCosto { get; set; }
		public int iIdOficina { get; set; }
		public DateTime? dtFechaCreacion { get; set; }
		public DateTime? dtFechaActualizacion { get; set; }
		public bool bActivo { get; set; }
		public Oficina Oficina { get; set; }
		public IEnumerable<OficinaPorTramite>? OficinaPorTramite { get; set; }

		public Tramite()
		{
			vchNombre = string.Empty;
			nvchDescripcion = string.Empty;
			dtFechaActualizacion = DateTime.Now;
			dtFechaCreacion = DateTime.Now;
			Oficina = new Oficina();

			bActivo = true;
		}
	}

}
