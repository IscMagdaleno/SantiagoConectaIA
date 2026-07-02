namespace SantiagoConectaIA.Share.PostClass.EventosModulo
{
    public class PostSaveSucursalEvento
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
    }
}
