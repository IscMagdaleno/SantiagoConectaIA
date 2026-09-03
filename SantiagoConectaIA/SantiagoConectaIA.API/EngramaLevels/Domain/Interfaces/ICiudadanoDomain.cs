using EngramaCoreStandar.Results;
using SantiagoConectaIA.Share.Objects.CiudadanoModule;
using SantiagoConectaIA.Share.PostModels.CiudadanoModule;
using System.Threading.Tasks;

namespace SantiagoConectaIA.API.EngramaLevels.Domain.Interfaces
{
    public interface ICiudadanoDomain
    {
        Task<Response<Ciudadano>> Registrar(PostSaveCiudadano postModel);
        Task<Response<Ciudadano>> Login(PostLoginCiudadano postModel);
        Task<Response<string>> EnviarCodigoWhatsApp(PostSendCodigoCiudadano postModel);
        Task<Response<Ciudadano>> ExternalLogin(PostExternalLoginCiudadano postModel);
        Task<Response<string>> GetSocialAuthId(string proveedor);
    }
}
