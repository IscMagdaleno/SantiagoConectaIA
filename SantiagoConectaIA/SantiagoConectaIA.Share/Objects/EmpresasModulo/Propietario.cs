using System;

namespace SantiagoConectaIA.Share.Objects.EmpresasModulo
{
    public class Propietario
    {
        public int iIdPropietario { get; set; }
        public string vchNombre { get; set; }
        public string vchCorreo { get; set; }
        public string vchTelefono { get; set; }
        public DateTime dtFechaCreacion { get; set; }
        public bool bActivo { get; set; }
    }
}
