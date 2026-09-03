using System.Collections.Generic;

namespace SantiagoConectaIA.Share.Objects.FeedModule
{
    public class FeedSearchResult
    {
        public List<FeedCard> Tramites { get; set; } = new();
        public List<FeedCard> Noticias { get; set; } = new();
        public List<FeedCard> Eventos { get; set; } = new();
        public List<FeedCard> Capsulas { get; set; } = new();
        public List<FeedCard> Publicaciones { get; set; } = new();
        public int iTotalRegistros { get; set; }
    }
}
