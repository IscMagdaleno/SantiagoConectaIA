using EngramaCoreStandar.Dapper;
using EngramaCoreStandar.Servicios;
using SantiagoConectaIA.API.EngramaLevels.Infrastructure.Entity.InformacionLocalModule;
using SantiagoConectaIA.API.EngramaLevels.Infrastructure.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SantiagoConectaIA.API.EngramaLevels.Infrastructure.Repository
{
    public class InformacionLocalRepository : IInformacionLocalRepository
    {
        private readonly IDapperManagerHelper _managerHelper;

        public InformacionLocalRepository(IDapperManagerHelper managerHelper)
        {
            _managerHelper = managerHelper;
        }

        public async Task<IEnumerable<spGetInformacionLocal.Result>> spGetInformacionLocal(spGetInformacionLocal.Request daoModel)
        {
            var respuesta = await _managerHelper.GetAllAsync<spGetInformacionLocal.Result, spGetInformacionLocal.Request>(daoModel, "", "SCIA");
            if (respuesta.Ok)
                return respuesta.Data;
            return new List<spGetInformacionLocal.Result> { new() { bResult = false, vchMessage = respuesta.Msg } };
        }

        public async Task<spGetInformacionLocalById.Result> spGetInformacionLocalById(spGetInformacionLocalById.Request daoModel)
        {
            var respuesta = await _managerHelper.GetAsync<spGetInformacionLocalById.Result, spGetInformacionLocalById.Request>(daoModel, "", "SCIA");
            if (respuesta.Ok)
                return respuesta.Data;
            return new spGetInformacionLocalById.Result { bResult = false, vchMessage = respuesta.Msg };
        }

        public async Task<spSaveInformacionLocal.Result> spSaveInformacionLocal(spSaveInformacionLocal.Request daoModel)
        {
            var respuesta = await _managerHelper.GetAsync<spSaveInformacionLocal.Result, spSaveInformacionLocal.Request>(daoModel, "", "SCIA");
            if (respuesta.Ok)
                return respuesta.Data;
            return new spSaveInformacionLocal.Result { bResult = false, vchMessage = respuesta.Msg };
        }
    }
}
