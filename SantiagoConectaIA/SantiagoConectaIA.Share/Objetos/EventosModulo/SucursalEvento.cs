namespace SantiagoConectaIA.Share.Objetos.EventosModulo
{
    public class SucursalEvento
    {
        public int iIdSucursalEvento { get; set; }
        public int iIdEvento { get; set; }
        public string vchNombre { get; set; }
        public string vchDireccion { get; set; }
        public double flLatitud { get; set; }
        public double flLongitud { get; set; }
        public string vchTelefono { get; set; }
        public string vchContacto { get; set; }
        public string vchHorario { get; set; }
        public string vchNotas { get; set; }
        public bool bActivo { get; set; } = true;
        public System.DateTime dtFechaRegistro { get; set; }
    }
}
