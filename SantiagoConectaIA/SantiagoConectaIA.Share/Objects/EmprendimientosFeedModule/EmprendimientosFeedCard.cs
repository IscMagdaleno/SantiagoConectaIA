using System;

namespace SantiagoConectaIA.Share.Objects.EmprendimientosFeedModule
{
    public class EmprendimientosFeedCard
    {
        public string vchTipoEntidad { get; set; } = string.Empty;
        public int iIdEntidad { get; set; }
        public int iIdEmpresa { get; set; }
        public string vchTitulo { get; set; } = string.Empty;
        public string nvchDescripcion { get; set; } = string.Empty;
        public string vchImagenUrl { get; set; } = string.Empty;
        public string vchNombreEmpresa { get; set; } = string.Empty;
        public decimal mPrecio { get; set; }
        public bool bAplicaDescuento { get; set; }
        public decimal mPrecioDescuento { get; set; }
        public int iTotalRegistros { get; set; }
        public string vchRutaDetalle { get; set; } = string.Empty;
    }
}
