namespace SantiagoConectaIA.Share.Objetos.EmpresasModulo
{
    public class EmpresaUbicacion
    {
        public int iIdUbicacion { get; set; } = -1;
        public int iIdEmpresa { get; set; } = -1;
        public string? vchAlias { get; set; }
        public string? vchDireccion { get; set; }
        public double flLatitud { get; set; }
        public double flLongitud { get; set; }
        public bool bActivo { get; set; } = true;
    }
}
