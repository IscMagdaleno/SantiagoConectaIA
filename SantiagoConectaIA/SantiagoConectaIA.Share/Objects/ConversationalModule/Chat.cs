using System;

namespace SantiagoConectaIA.Share.Objects.ConversationalModule
{
	public class Chat
	{
		public int iIdChat { get; set; }
		public int iIdProyecto { get; set; }
		public DateTime dtFechaCreacion { get; set; }
		public string nvchNombre { get; set; }
		public bool bActivo { get; set; }
		public string nvchThreadId { get; set; }

		public Chat()
		{
			nvchNombre = string.Empty;
			nvchThreadId = string.Empty;
			dtFechaCreacion = DateTime.Now;
			bActivo = true;
		}
	}
}
