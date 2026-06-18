namespace SantiagoConectaIA.Share.Objetos.EmpresasModulo
{
    public class EmpresaUbicacion
    {
        public int iIdUbicacion { get; set; }
        public int iIdEmpresa { get; set; }
        public string vchAlias { get; set; }
        public string vchDireccion { get; set; }
        public double? flLatitud { get; set; }
        public double? flLongitud { get; set; }
    }
}
