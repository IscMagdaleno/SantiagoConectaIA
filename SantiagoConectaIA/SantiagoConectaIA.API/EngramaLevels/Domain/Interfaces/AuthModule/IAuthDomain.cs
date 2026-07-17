using EngramaCoreStandar.Results;

using SantiagoConectaIA.Share.Objects.AuthModulo;
using SantiagoConectaIA.Share.PostModels.AuthModulo;

using System.Threading.Tasks;

namespace SantiagoConectaIA.API.EngramaLevels.Domain.Interfaces.AuthModule
{
    public interface IAuthDomain
    {
        Task<Response<UsuarioAuth>> Login(PostLoginUsuario daoModel);
        Task<Response<UsuarioAuth>> SaveUsuario(PostSaveUsuario daoModel);
    }
}
