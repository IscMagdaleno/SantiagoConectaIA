using EngramaCoreStandar.Dapper;
using SantiagoConectaIA.API.EngramaLevels.Infrastructure.Entity.OpinionModule;
using SantiagoConectaIA.API.EngramaLevels.Infrastructure.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SantiagoConectaIA.API.EngramaLevels.Infrastructure.Repository
{
    public class OpinionRepository : IOpinionRepository
    {
        private readonly IDapperManagerHelper _managerHelper;

        public OpinionRepository(IDapperManagerHelper managerHelper)
        {
            _managerHelper = managerHelper;
        }

        public async Task<IEnumerable<spGetOpiniones.Result>> spGetOpiniones(spGetOpiniones.Request daoModel)
        {
            var respuesta = await _managerHelper.GetAllAsync<spGetOpiniones.Result, spGetOpiniones.Request>(daoModel, "", "SCIA");
            if (respuesta.Ok)
            {
                return respuesta.Data;
            }
            return new List<spGetOpiniones.Result> { new() { bResult = false, vchMessage = respuesta.Msg } };
        }

        public async Task<spSaveOpinion.Result> spSaveOpinion(spSaveOpinion.Request daoModel)
        {
            var respuesta = await _managerHelper.GetAsync<spSaveOpinion.Result, spSaveOpinion.Request>(daoModel, "", "SCIA");
            if (respuesta.Ok)
            {
                return respuesta.Data;
            }
            return new spSaveOpinion.Result { bResult = false, vchMessage = respuesta.Msg };
        }
    }
}
