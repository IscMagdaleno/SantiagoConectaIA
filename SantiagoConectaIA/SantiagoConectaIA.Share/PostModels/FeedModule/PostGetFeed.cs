namespace SantiagoConectaIA.Share.PostModels.FeedModule
{
    public class PostGetFeed
    {
        public int iPage { get; set; } = 1;
        public int iPageSize { get; set; } = 10;
        public string? vchSessionSeed { get; set; }
        /// <summary>
        /// TODO | TRAMITE | NOTICIA | EVENTO | CAPSULA
        /// </summary>
        public string? vchTipoFiltro { get; set; }
    }
}
