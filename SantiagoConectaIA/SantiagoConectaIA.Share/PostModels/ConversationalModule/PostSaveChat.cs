using System;

namespace SantiagoConectaIA.Share.PostModels.ConversationalModule
{
	public class PostSaveChat
	{
		public int iIdChat { get; set; }
		public int iIdProyecto { get; set; }
		public DateTime dtFechaCreacion { get; set; }
		public string nvchNombre { get; set; }
		public bool bActivo { get; set; }
		public string nvchThreadId { get; set; }

		public PostSaveChat()
		{
			nvchNombre = string.Empty;
			nvchThreadId = string.Empty;
			dtFechaCreacion = DateTime.Now;
			bActivo = true;
		}
	}
}
