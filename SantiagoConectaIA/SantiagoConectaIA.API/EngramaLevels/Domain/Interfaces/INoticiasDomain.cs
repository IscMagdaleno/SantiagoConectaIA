using SantiagoConectaIA.API.EngramaLevels.Infrastructure.Entity.NoticiasModule;
using SantiagoConectaIA.Share.Objects.NoticiasModule;
using SantiagoConectaIA.Share.PostModels.NoticiasModule;
using EngramaCoreStandar.Results;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SantiagoConectaIA.API.EngramaLevels.Domain.Interfaces
{
    public interface INoticiasDomain
    {
        Task<Response<IEnumerable<Noticia>>> GetNoticias(PostGetNoticias postModel);
        Task<Response<Noticia>> SaveNoticia(PostSaveNoticia postModel);
    }
}
