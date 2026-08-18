using SantiagoConectaIA.API.EngramaLevels.Infrastructure.Entity.CiudadanoModule;
using System.Threading.Tasks;

namespace SantiagoConectaIA.API.EngramaLevels.Infrastructure.Interfaces
{
    public interface ICiudadanoRepository
    {
        Task<spSaveCiudadano.Result> spSaveCiudadano(spSaveCiudadano.Request daoModel);
        Task<spGetCiudadanoAuth.Result> spGetCiudadanoAuth(spGetCiudadanoAuth.Request daoModel);
    }
}
