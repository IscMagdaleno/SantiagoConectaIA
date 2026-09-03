namespace SantiagoConectaIA.Share.Objects.CiudadanoModule
{
    public class Ciudadano
    {
        public int iIdCiudadano { get; set; }
        public string vchAlias { get; set; } = string.Empty;
        public string vchTelefono { get; set; } = string.Empty;
        public string? vchEmail { get; set; }
        public string? vchAvatarUrl { get; set; }
        public string vchProveedorAuth { get; set; } = "Local";
        public bool bCuentaVerificada { get; set; } = true;
        public string Token { get; set; } = string.Empty;
        public string vchRol { get; set; } = "Ciudadano";
    }
}
