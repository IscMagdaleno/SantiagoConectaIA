using System;

namespace SantiagoConectaIA.Share.PostModels.ConversationalModule
{
	public class PostSaveMensaje
	{
		public int iIdMensaje { get; set; }
		public int iIdChat { get; set; }
		public int iOrden { get; set; }
		public int iIdFase { get; set; }
		public string nvchRol { get; set; }
		public string nvchContenido { get; set; }
		public DateTime dtFecha { get; set; }
		public string nvchThreadId { get; set; }

		public PostSaveMensaje()
		{
			nvchRol = string.Empty;
			nvchContenido = string.Empty;
			dtFecha = DateTime.Now;
			nvchThreadId = string.Empty;
		}
	}
}
