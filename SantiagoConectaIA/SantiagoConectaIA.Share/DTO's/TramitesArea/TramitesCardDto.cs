using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SantiagoConectaIA.Share.DTO_s.TramitesArea
{
	public class TramitesCardDto
	{
		public int iIdTramite { get; set; }
		public int iIdOficina { get; set; }
		public int iIdCategoria { get; set; }
		public string vchNombreTramite { get; set; }
		public string vchDescripcionTramite { get; set; }
		public string vchNombreOficina { get; set; }
		public string vchDireccionOficina { get; set; }
		public string vchTelefonoOficina { get; set; }
		public bool bModalidadEnLinea { get; set; }
		public decimal dCosto { get; set; }
		public string vchHorarioOficina { get; set; }
	}
}
