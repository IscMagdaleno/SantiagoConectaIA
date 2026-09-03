using SantiagoConectaIA.API.EngramaLevels.Infrastructure.Entity.EventosModule;
using SantiagoConectaIA.API.EngramaLevels.Infrastructure.Entity.PublicacionesCiudadanoModule;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SantiagoConectaIA.API.EngramaLevels.Infrastructure.Interfaces
{
    public interface IPublicacionesCiudadanoRepository
    {
        Task<spSavePublicacionCiudadano.Result> spSavePublicacionCiudadano(spSavePublicacionCiudadano.Request daoModel);
        Task<spSaveImagenRegistro.Result> spSaveImagenRegistro(spSaveImagenRegistro.Request daoModel);
        Task<IEnumerable<spGetPublicacionesCiudadano.Result>> spGetPublicacionesCiudadano(spGetPublicacionesCiudadano.Request daoModel);
        Task<IEnumerable<spGetMisPublicacionesCiudadano.Result>> spGetMisPublicacionesCiudadano(spGetMisPublicacionesCiudadano.Request daoModel);
        Task<spDeletePublicacionCiudadano.Result> spDeletePublicacionCiudadano(spDeletePublicacionCiudadano.Request daoModel);
    }
}
