using System;

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
    }
}
