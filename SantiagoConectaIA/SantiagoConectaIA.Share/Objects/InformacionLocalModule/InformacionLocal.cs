using System;

namespace SantiagoConectaIA.Share.Objects.InformacionLocalModule
{
    public class InformacionLocal
    {
        public int iIdInformacionLocal { get; set; }
        public string nvchCategoria { get; set; } = string.Empty;
        public string nvchTitulo { get; set; } = string.Empty;
        public string? nvchPalabrasClave { get; set; }
        public string nvchDescripcionCorta { get; set; } = string.Empty;
        public string? nvchContenidoDetallado { get; set; }
        public string? nvchUbicacion_LatLong { get; set; }
        public DateTime dtFechaCreacion { get; set; } = DateTime.Now;
        public bool bActivo { get; set; } = true;
    }
}
