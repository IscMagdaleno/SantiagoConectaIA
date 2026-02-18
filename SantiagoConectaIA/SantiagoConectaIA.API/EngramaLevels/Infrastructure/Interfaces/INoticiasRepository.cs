using SantiagoConectaIA.API.EngramaLevels.Infrastructure.Entity.NoticiasModule;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SantiagoConectaIA.API.EngramaLevels.Infrastructure.Interfaces
{
    public interface INoticiasRepository
    {
        Task<IEnumerable<spGetNoticias.Result>> spGetNoticias(spGetNoticias.Request daoModel);
        Task<spSaveNoticia.Result> spSaveNoticia(spSaveNoticia.Request daoModel);
        Task<spSaveNoticiaImagen.Result> spSaveNoticiaImagen(spSaveNoticiaImagen.Request daoModel);
    }
}
