namespace SantiagoConectaIA.Share.PostModels.FeedModule
{
	public class PostGetFeed
	{
		public int iPage { get; set; } = 1;
		public int iPageSize { get; set; } = 10;
		public string? vchSessionSeed { get; set; }
		/// <summary>
		/// TODO | PUBLICACION | NOTICIA | EVENTO | CAPSULA
		/// </summary>
		public string? vchTipoFiltro { get; set; }
		/// <summary>
		/// true = omite lo de hoy (publicaciones, noticias y eventos del día) y devuelve solo contenido aleatorio.
		/// El cliente lo usa para seguir mostrando contenido cuando ya se recorrió todo lo anterior.
		/// </summary>
		public bool bSoloAleatorio { get; set; }
	}
}
