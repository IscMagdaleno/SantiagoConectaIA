using System.Collections.Generic;
using System.Threading.Tasks;
using EngramaCoreStandar.Results;
using SantiagoConectaIA.Share.Objects.CatalogosModule;
using SantiagoConectaIA.Share.PostModels.CatalogosModule;

namespace SantiagoConectaIA.API.EngramaLevels.Domain.Interfaces
{
    public interface ICatalogosDomain
    {
        Task<Response<IEnumerable<Catalogo>>> GetCatalogos(PostGetCatalogos postModel);
        Task<Response<Parametro>> GetParametroByAlias(PostGetParametro postModel);
    }
}
