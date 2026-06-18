using System;

namespace SantiagoConectaIA.Share.Objetos.EmpresasModulo
{
    public class Empresa
    {
        public int iIdEmpresa { get; set; }
        public int? iIdCatalogoEmpresa { get; set; }
        public string vchNombreComercial { get; set; }
        public string? vchSlogan { get; set; }
        public string? vchLogoUrl { get; set; }
        public string? nvchDescripcion { get; set; }
        public string? nvchMision { get; set; }
        public string? nvchVision { get; set; }
        public string? nvchHistoria { get; set; }
        public string? vchTelefono { get; set; }
        public string? vchCorreo { get; set; }
        public bool bEstatus { get; set; }
        public DateTime dtFechaRegistro { get; set; }
    }
}
