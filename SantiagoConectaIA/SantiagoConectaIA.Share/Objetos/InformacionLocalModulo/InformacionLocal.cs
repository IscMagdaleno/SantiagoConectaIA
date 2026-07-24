using System;

namespace SantiagoConectaIA.Share.Objetos.InformacionLocalModulo
{
    public class InformacionLocal
    {
        public int iIdInformacionLocal { get; set; }
        public string nvchCategoria { get; set; }
        public string nvchTitulo { get; set; }
        public string nvchPalabrasClave { get; set; }
        public string nvchDescripcionCorta { get; set; }
        public string nvchContenidoDetallado { get; set; }
        public string nvchUbicacion_LatLong { get; set; }
        public DateTime dtFechaCreacion { get; set; }
        public bool bActivo { get; set; }
    }
}
