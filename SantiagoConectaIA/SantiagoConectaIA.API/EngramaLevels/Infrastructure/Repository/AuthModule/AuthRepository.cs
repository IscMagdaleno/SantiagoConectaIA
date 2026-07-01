using EngramaCoreStandar.Dapper;
using SantiagoConectaIA.API.EngramaLevels.Infrastructure.Entity.AuthModule;
using SantiagoConectaIA.API.EngramaLevels.Infrastructure.Interfaces.AuthModule;
using System.Threading.Tasks;

namespace SantiagoConectaIA.API.EngramaLevels.Infrastructure.Repository.AuthModule
{
    public class AuthRepository : IAuthRepository
    {
        private readonly IDapperManagerHelper _managerHelper;

        public AuthRepository(IDapperManagerHelper managerHelper)
        {
            _managerHelper = managerHelper;
        }

        public async Task<spGetUsuarioAuth.Result> spGetUsuarioAuth(spGetUsuarioAuth.Request request)
        {
            var respuesta = await _managerHelper.GetAsync<spGetUsuarioAuth.Result, spGetUsuarioAuth.Request>(request, "", "SCIA");
            if (respuesta.Ok)
            {
                return respuesta.Data;
            }
            return new spGetUsuarioAuth.Result { bResult = false, vchMessage = respuesta.Msg };
        }

        public async Task<spSaveUsuario.Result> spSaveUsuario(spSaveUsuario.Request request)
        {
            var respuesta = await _managerHelper.GetAsync<spSaveUsuario.Result, spSaveUsuario.Request>(request, "", "SCIA");
            if (respuesta.Ok)
            {
                return respuesta.Data;
            }
            return new spSaveUsuario.Result { bResult = false, vchMessage = respuesta.Msg };
        }
    }
}
