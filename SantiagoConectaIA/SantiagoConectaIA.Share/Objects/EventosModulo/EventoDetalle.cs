using System;
using System.Collections.Generic;

namespace SantiagoConectaIA.Share.Objects.EventosModulo
{
    public class EventoDetalle
    {
        public int iIdEvento { get; set; }
        public int? iIdCategoriaEvento { get; set; }
        public string vchCategoriaNombre { get; set; }
        public string vchNombre { get; set; }
        public string nvchDescripcion { get; set; }
        public DateTime dtFechaInicio { get; set; }
        public DateTime? dtFechaFin { get; set; }
        public string vchLugar { get; set; }
        public string vchDireccion { get; set; }
        public double flLatitud { get; set; }
        public double flLongitud { get; set; }
        public string vchImagenPortada { get; set; }
        public string vchCostoTexto { get; set; }
        public string vchOrganizador { get; set; }
        public string vchTelefono { get; set; }
        public string vchCorreo { get; set; }
        public string vchUrlOficial { get; set; }
        public bool bDestacado { get; set; }
        public bool bEstatus { get; set; }
        public DateTime dtFechaRegistro { get; set; }
        public List<ImagenRegistro> imagenes { get; set; } = new();
    }
}
