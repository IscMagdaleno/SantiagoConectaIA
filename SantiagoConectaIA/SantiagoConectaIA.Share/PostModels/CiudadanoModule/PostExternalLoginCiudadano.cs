namespace SantiagoConectaIA.Share.PostModels.CiudadanoModule
{
    public class PostExternalLoginCiudadano
    {
        public string vchProveedor { get; set; } = string.Empty; // "Google", "Facebook"
        public string vchToken { get; set; } = string.Empty;     // ID token (Google) o Access token (Facebook)
        public string? vchIdProveedor { get; set; }
        public string? vchEmail { get; set; }
        public string? vchAlias { get; set; }
        public string? vchAvatarUrl { get; set; }
    }
}
