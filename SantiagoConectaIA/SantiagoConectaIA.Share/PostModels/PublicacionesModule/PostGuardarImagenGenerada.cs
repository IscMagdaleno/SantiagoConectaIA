namespace SantiagoConectaIA.Share.PostModels.PublicacionesModule
{
    public class PostGuardarImagenGenerada
    {
        /// <summary>
        /// Imagen en formato Base64 (data:image/png;base64,... o solo base64)
        /// </summary>
        public string Base64Data { get; set; } = string.Empty;

        /// <summary>
        /// Título o prefijo para nombrar el archivo
        /// </summary>
        public string Titulo { get; set; } = "post-social";
    }
}
