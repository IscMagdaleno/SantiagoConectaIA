namespace SantiagoConectaIA.Share.PostModels.PublicacionesModule
{
    public class PostMejorarProducto
    {
        public int iIdEmpresa { get; set; }
        public int iIdProducto { get; set; }
        public string vchNombreComercial { get; set; } = string.Empty;
        public string vchNombreProducto { get; set; } = string.Empty;
        public string nvchInformacion { get; set; } = string.Empty;
    }
}
