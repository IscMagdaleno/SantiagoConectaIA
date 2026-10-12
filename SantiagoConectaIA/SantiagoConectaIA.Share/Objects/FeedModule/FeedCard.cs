namespace SantiagoConectaIA.Share.Objects.FeedModule
{
	public class FeedCard
	{
		public string vchTipoEntidad { get; set; } = string.Empty;
		public int iIdEntidad { get; set; }
		public string vchTitulo { get; set; } = string.Empty;
		public string nvchDescripcion { get; set; } = string.Empty;
		public string? nvchContenidoDetallado { get; set; }
		public string vchImagenUrl { get; set; } = string.Empty;
		public DateTime? dtFecha { get; set; }
		public int iTotalRegistros { get; set; }
		public string vchRutaDetalle { get; set; } = string.Empty;
		public string? nvchImagenesJson { get; set; }
		public System.Collections.Generic.List<string> ImagenesUrls { get; set; } = new();
		public int iTotalComentarios { get; set; }
		/// <summary>Alias del ciudadano que publicó (solo PUBLICACION).</summary>
		public string vchAutor { get; set; } = string.Empty;
		public string? vchAvatarUrl { get; set; }
		public bool bAutorVerificado { get; set; }
		/// <summary>Categoría de la publicación, lugar del evento o categoría del dato curioso.</summary>
		public string vchEtiqueta { get; set; } = string.Empty;
		/// <summary>1 = publicación de ciudadano de hoy; 2 = noticia o evento del día; 3 = descubrimiento aleatorio.</summary>
		public int iPrioridad { get; set; } = 3;
	}
}
