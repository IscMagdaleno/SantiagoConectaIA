namespace SantiagoConectaIA.Share.PostModels.PublicacionesModule
{
    public class PublicacionAutomatica
    {
        public bool bActivo { get; set; } = true;
        public int iHora { get; set; } = 11;
        public int? iIdUltimaNoticia { get; set; }
        public string? vchUltimaNoticia { get; set; }
        public DateTime? dtUltimaPublicacion { get; set; }
    }
}
