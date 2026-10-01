namespace SantiagoConectaIA.Share.PostModels.PublicacionesModule
{
    public class PublicacionAutomaticaEmprendimientos
    {
        public bool bActivo { get; set; } = true;
        public int iHora { get; set; }
        public int iIdSiguienteEmpresa { get; set; }
        public string? vchSiguienteEmpresa { get; set; }
        public int? iIdUltimaEmpresa { get; set; }
        public string? vchUltimaEmpresa { get; set; }
        public DateTime? dtUltimaPublicacion { get; set; }
    }
}
