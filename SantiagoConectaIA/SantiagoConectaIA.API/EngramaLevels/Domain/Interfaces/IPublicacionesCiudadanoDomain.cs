using EngramaCoreStandar.Results;
using SantiagoConectaIA.Share.Objects.PublicacionCiudadanoModule;
using SantiagoConectaIA.Share.PostModels.PublicacionCiudadanoModule;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SantiagoConectaIA.API.EngramaLevels.Domain.Interfaces
{
    public interface IPublicacionesCiudadanoDomain
    {
        Task<Response<PublicacionCiudadano>> SavePublicacionCiudadano(PostSavePublicacionCiudadano postModel, int iIdCiudadano);
        Task<Response<IEnumerable<PublicacionCiudadano>>> GetPublicacionesCiudadano(PostGetPublicacionesCiudadano postModel);
        Task<Response<IEnumerable<PublicacionCiudadano>>> GetMisPublicacionesCiudadano(PostGetMisPublicacionesCiudadano postModel, int iIdCiudadano);
        Task<Response<string>> DeletePublicacionCiudadano(PostDeletePublicacionCiudadano postModel, int iIdCiudadano);
    }
}
