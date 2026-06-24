using System;

namespace SantiagoConectaIA.Share.Objects.ConversationalModule
{
	public class Mensaje
	{
		public int iIdMensaje { get; set; }
		public int iIdChat { get; set; }
		public int iOrden { get; set; }
		public string nvchRol { get; set; }
		public string nvchContenido { get; set; }
		public DateTime dtFecha { get; set; }

		public Mensaje()
		{
			nvchRol = string.Empty;
			nvchContenido = string.Empty;
			dtFecha = DateTime.Now;
		}
	}
}
