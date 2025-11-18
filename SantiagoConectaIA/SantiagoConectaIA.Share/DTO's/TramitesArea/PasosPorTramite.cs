using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SantiagoConectaIA.Share.DTO_s.TramitesArea
{
	public class PasosPorTramite
	{
		public int iIdTramitePaso { get; set; }
		public int iIdTramite { get; set; }
		public short iOrden { get; set; }
		public string nvchDescripcion { get; set; }
		public bool bActivo { get; set; }

		public PasosPorTramite()
		{
			nvchDescripcion = string.Empty;
		}
	}
}
