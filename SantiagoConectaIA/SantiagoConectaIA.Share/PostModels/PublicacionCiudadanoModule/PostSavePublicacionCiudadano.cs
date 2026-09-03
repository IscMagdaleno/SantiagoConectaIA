using System.Collections.Generic;

namespace SantiagoConectaIA.Share.PostModels.PublicacionCiudadanoModule
{
    public class PostSavePublicacionCiudadano
    {
        public int iIdPublicacion { get; set; } = 0;
        public string? nvchTitulo { get; set; }
        public string nvchContenidoTexto { get; set; } = string.Empty;
        public string vchCategoriaPublicacion { get; set; } = "Comunidad";
        public List<string> ImagenesUrls { get; set; } = new();
        /// <summary>
        /// Imágenes enviadas en Base64 para ser procesadas y subidas a Azure Blob Storage.
        /// Formato: "data:image/jpeg;base64,..."
        /// </summary>
        public List<string>? ImagenesBase64 { get; set; }
    }
}
