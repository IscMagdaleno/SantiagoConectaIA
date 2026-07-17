namespace SantiagoConectaIA.Share.Objects.EmpresasModulo
{
    public class EmpresaRedSocial
    {
        public int iIdRedSocial { get; set; } = -1;
        public int iIdEmpresa { get; set; } = -1;
        public string? vchPlataforma { get; set; } = string.Empty;
        public string? vchUrl { get; set; } = string.Empty;
        public bool bActivo { get; set; } = true;
    }
}
