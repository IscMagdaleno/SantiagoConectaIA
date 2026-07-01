namespace SantiagoConectaIA.Share.PostModels.AuthModulo
{
    public class PostSaveUsuario
    {
        public int iIdUsuario { get; set; }
        public int iIdRol { get; set; }
        public string vchNombre { get; set; }
        public string vchUserName { get; set; }
        public string vchEmail { get; set; }
        public string vchPassword { get; set; }
        public bool bActivo { get; set; }
    }
}
