namespace SantiagoConectaIA.Share.PostModels.OpinionModule
{
    public class PostSaveOpinion
    {
        public string vchTipoEntidad { get; set; } = string.Empty;
        public int iIdEntidad { get; set; }
        public int? iIdOpinionPadre { get; set; }
        public string nvchTexto { get; set; } = string.Empty;
    }
}
