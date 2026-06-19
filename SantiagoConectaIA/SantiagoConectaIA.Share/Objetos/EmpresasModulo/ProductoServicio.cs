namespace SantiagoConectaIA.Share.Objetos.EmpresasModulo
{
    public class ProductoServicio
    {
        public int iIdProducto { get; set; } = -1;
        public int iIdCategoriaCat { get; set; } = -1;
        public string? vchNombre { get; set; } = string.Empty;
        public string? nvchDescripcionCorta { get; set; } = string.Empty;
        public decimal mPrecio { get; set; } = 0;
        public string? vchImagenUrl { get; set; } = string.Empty;
        public bool bAplicaDescuento { get; set; } = false;
        public decimal mPrecioDescuento { get; set; } = 0;
        public bool bEstatus { get; set; } = true;
    }
}
