namespace SantiagoConectaIA.Share.Objects.CiudadanoModule
{
    public class Ciudadano
    {
        public int iIdCiudadano { get; set; }
        public string vchAlias { get; set; } = string.Empty;
        public string vchTelefono { get; set; } = string.Empty;
        public string Token { get; set; } = string.Empty;
        public string vchRol { get; set; } = "Ciudadano";
    }
}
