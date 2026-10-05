using System;

namespace SantiagoConectaIA.Share.Objects.EventosModulo
{
    public class EventoExtraidoDto
    {
        public string vchNombre { get; set; } = string.Empty;
        public string nvchDescripcion { get; set; } = string.Empty;
        public DateTime dtFechaInicio { get; set; }
        public DateTime? dtFechaFin { get; set; }
        public string vchLugar { get; set; } = string.Empty;
        public string vchDireccion { get; set; } = string.Empty;
        public string vchCostoTexto { get; set; } = string.Empty;
        public string vchOrganizador { get; set; } = string.Empty;
        public string vchTelefono { get; set; } = string.Empty;
        public string vchCorreo { get; set; } = string.Empty;
        public string vchUrlOficial { get; set; } = string.Empty;
        public string vchImagenPortada { get; set; } = string.Empty;
        public double flLatitud { get; set; }
        public double flLongitud { get; set; }
    }
}
