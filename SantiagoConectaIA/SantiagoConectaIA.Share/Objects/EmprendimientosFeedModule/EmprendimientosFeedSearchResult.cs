using System.Collections.Generic;

namespace SantiagoConectaIA.Share.Objects.EmprendimientosFeedModule
{
    public class EmprendimientosFeedSearchResult
    {
        public List<EmprendimientosFeedCard> Emprendimientos { get; set; } = new();
        public List<EmprendimientosFeedCard> Productos { get; set; } = new();
        public int iTotalRegistros { get; set; }
    }
}
