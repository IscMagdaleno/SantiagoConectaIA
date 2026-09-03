using System;

namespace SantiagoConectaIA.Share.Objects.PublicacionCiudadanoModule
{
    public class ImagenPublicacion
    {
        public int iIdImagen { get; set; }
        public int iIdPublicacion { get; set; }
        public string nvchUrlImagen { get; set; } = string.Empty;
        public int iOrdenVisualizacion { get; set; } = 1;
        public DateTime dtFechaCreacion { get; set; } = DateTime.UtcNow;
    }
}
