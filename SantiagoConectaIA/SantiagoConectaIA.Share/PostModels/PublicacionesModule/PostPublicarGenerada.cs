namespace SantiagoConectaIA.Share.PostModels.PublicacionesModule
{
    /// <summary>
    /// Publicación ya preparada en el navegador (texto con IA e imagen del post capturada).
    /// El servidor valida que sea realmente la siguiente por publicar y la registra.
    /// </summary>
    public class PostPublicarGenerada
    {
        /// <summary>ID de la noticia o del emprendimiento que se publica.</summary>
        public int iIdRegistro { get; set; }
        public string Message { get; set; } = string.Empty;
        public string ImageUrl { get; set; } = string.Empty;
    }
}
