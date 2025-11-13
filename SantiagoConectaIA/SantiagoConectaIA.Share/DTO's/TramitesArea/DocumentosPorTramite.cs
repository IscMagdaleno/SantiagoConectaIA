using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SantiagoConectaIA.Share.DTO_s.TramitesArea
{
	public class DocumentosPorTramite
	{
		public int iIdDocumento { get; set; }
		public int iIdTramite { get; set; }
		public string vchNombre { get; set; }
		public string vchUrl { get; set; }
		public bool bActivo { get; set; }

		public DocumentosPorTramite()
		{
			vchNombre = string.Empty;
			vchUrl = string.Empty;
		}
	}
}
