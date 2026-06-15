using System.Collections.Generic;
using System.Threading.Tasks;
using SantiagoConectaIA.API.EngramaLevels.Infrastructure.Entity.CatalogosModule;

namespace SantiagoConectaIA.API.EngramaLevels.Infrastructure.Interfaces
{
    public interface ICatalogosRepository
    {
        Task<IEnumerable<spGetCatalogos.Result>> spGetCatalogos(spGetCatalogos.Request request);
        Task<spGetParametro.Result> spGetParametro(spGetParametro.Request request);
    }
}
