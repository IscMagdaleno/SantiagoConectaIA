using EngramaCoreStandar.Dapper;
using SantiagoConectaIA.API.EngramaLevels.Infrastructure.Entity.EventosModule;
using SantiagoConectaIA.API.EngramaLevels.Infrastructure.Entity.PublicacionesCiudadanoModule;
using SantiagoConectaIA.API.EngramaLevels.Infrastructure.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SantiagoConectaIA.API.EngramaLevels.Infrastructure.Repository
{
    public class PublicacionesCiudadanoRepository : IPublicacionesCiudadanoRepository
    {
        private readonly IDapperManagerHelper _managerHelper;

        public PublicacionesCiudadanoRepository(IDapperManagerHelper managerHelper)
        {
            _managerHelper = managerHelper;
        }

        public async Task<spSavePublicacionCiudadano.Result> spSavePublicacionCiudadano(spSavePublicacionCiudadano.Request daoModel)
        {
            var respuesta = await _managerHelper.GetAsync<spSavePublicacionCiudadano.Result, spSavePublicacionCiudadano.Request>(daoModel, "", "SCIA");
            if (respuesta.Ok)
            {
                return respuesta.Data;
            }
            return new spSavePublicacionCiudadano.Result { bResult = false, vchMessage = respuesta.Msg };
        }

        public async Task<spSaveImagenRegistro.Result> spSaveImagenRegistro(spSaveImagenRegistro.Request daoModel)
        {
            var respuesta = await _managerHelper.GetAsync<spSaveImagenRegistro.Result, spSaveImagenRegistro.Request>(daoModel, "", "SCIA");
            if (respuesta.Ok)
            {
                return respuesta.Data;
            }
            return new spSaveImagenRegistro.Result { bResult = false, vchMessage = respuesta.Msg };
        }

        public async Task<IEnumerable<spGetPublicacionesCiudadano.Result>> spGetPublicacionesCiudadano(spGetPublicacionesCiudadano.Request daoModel)
        {
            var respuesta = await _managerHelper.GetAllAsync<spGetPublicacionesCiudadano.Result, spGetPublicacionesCiudadano.Request>(daoModel, "", "SCIA");
            if (respuesta.Ok)
            {
                return respuesta.Data;
            }
            return new List<spGetPublicacionesCiudadano.Result> { new() { bResult = false, vchMessage = respuesta.Msg } };
        }

        public async Task<IEnumerable<spGetMisPublicacionesCiudadano.Result>> spGetMisPublicacionesCiudadano(spGetMisPublicacionesCiudadano.Request daoModel)
        {
            var respuesta = await _managerHelper.GetAllAsync<spGetMisPublicacionesCiudadano.Result, spGetMisPublicacionesCiudadano.Request>(daoModel, "", "SCIA");
            if (respuesta.Ok)
            {
                return respuesta.Data;
            }
            return new List<spGetMisPublicacionesCiudadano.Result> { new() { bResult = false, vchMessage = respuesta.Msg } };
        }

        public async Task<spDeletePublicacionCiudadano.Result> spDeletePublicacionCiudadano(spDeletePublicacionCiudadano.Request daoModel)
        {
            var respuesta = await _managerHelper.GetAsync<spDeletePublicacionCiudadano.Result, spDeletePublicacionCiudadano.Request>(daoModel, "", "SCIA");
            if (respuesta.Ok)
            {
                return respuesta.Data;
            }
            return new spDeletePublicacionCiudadano.Result { bResult = false, vchMessage = respuesta.Msg };
        }
    }
}
