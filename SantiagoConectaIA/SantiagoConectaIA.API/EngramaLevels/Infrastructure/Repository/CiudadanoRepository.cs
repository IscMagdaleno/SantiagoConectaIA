using EngramaCoreStandar.Dapper;
using SantiagoConectaIA.API.EngramaLevels.Infrastructure.Entity.CiudadanoModule;
using SantiagoConectaIA.API.EngramaLevels.Infrastructure.Interfaces;
using System.Threading.Tasks;

namespace SantiagoConectaIA.API.EngramaLevels.Infrastructure.Repository
{
    public class CiudadanoRepository : ICiudadanoRepository
    {
        private readonly IDapperManagerHelper _managerHelper;

        public CiudadanoRepository(IDapperManagerHelper managerHelper)
        {
            _managerHelper = managerHelper;
        }

        public async Task<spSaveCiudadano.Result> spSaveCiudadano(spSaveCiudadano.Request daoModel)
        {
            var respuesta = await _managerHelper.GetAsync<spSaveCiudadano.Result, spSaveCiudadano.Request>(daoModel, "", "SCIA");
            if (respuesta.Ok)
            {
                return respuesta.Data;
            }
            return new spSaveCiudadano.Result { bResult = false, vchMessage = respuesta.Msg };
        }

        public async Task<spGetCiudadanoAuth.Result> spGetCiudadanoAuth(spGetCiudadanoAuth.Request daoModel)
        {
            var respuesta = await _managerHelper.GetAsync<spGetCiudadanoAuth.Result, spGetCiudadanoAuth.Request>(daoModel, "", "SCIA");
            if (respuesta.Ok)
            {
                return respuesta.Data;
            }
            return new spGetCiudadanoAuth.Result { bResult = false, vchMessage = respuesta.Msg };
        }

        public async Task<spSaveCiudadanoCodigo.Result> spSaveCiudadanoCodigo(spSaveCiudadanoCodigo.Request daoModel)
        {
            var respuesta = await _managerHelper.GetAsync<spSaveCiudadanoCodigo.Result, spSaveCiudadanoCodigo.Request>(daoModel, "", "SCIA");
            if (respuesta.Ok)
            {
                return respuesta.Data;
            }
            return new spSaveCiudadanoCodigo.Result { bResult = false, vchMessage = respuesta.Msg };
        }

        public async Task<spValidarCiudadanoCodigo.Result> spValidarCiudadanoCodigo(spValidarCiudadanoCodigo.Request daoModel)
        {
            var respuesta = await _managerHelper.GetAsync<spValidarCiudadanoCodigo.Result, spValidarCiudadanoCodigo.Request>(daoModel, "", "SCIA");
            if (respuesta.Ok)
            {
                return respuesta.Data;
            }
            return new spValidarCiudadanoCodigo.Result { bResult = false, vchMessage = respuesta.Msg };
        }

        public async Task<spSaveCiudadanoExternalLogin.Result> spSaveCiudadanoExternalLogin(spSaveCiudadanoExternalLogin.Request daoModel)
        {
            var respuesta = await _managerHelper.GetAsync<spSaveCiudadanoExternalLogin.Result, spSaveCiudadanoExternalLogin.Request>(daoModel, "", "SCIA");
            if (respuesta.Ok)
            {
                return respuesta.Data;
            }
            return new spSaveCiudadanoExternalLogin.Result { bResult = false, vchMessage = respuesta.Msg };
        }
    }
}
