using SantiagoConectaIA.Share.Objects.TramitesModule;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SantiagoConectaIA.Share.DTO_s.TramitesArea
{
	public class TramiteDetalleDto : Tramite
	{
		public List<RequisitosPorTramite> Requisitos { get; set; }
		public List<PasosPorTramite> Pasos { get; set; }
		public List<DocumentosPorTramite> Documentos { get; set; }

		public TramiteDetalleDto() : base() 
		{
			Requisitos = new List<RequisitosPorTramite>();
			Pasos = new List<PasosPorTramite>();
			Documentos = new List<DocumentosPorTramite>();
		}
	}
}
