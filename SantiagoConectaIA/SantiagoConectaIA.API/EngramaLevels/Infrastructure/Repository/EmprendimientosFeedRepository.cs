using EngramaCoreStandar.Dapper;
using SantiagoConectaIA.API.EngramaLevels.Infrastructure.Entity.EmprendimientosFeedModule;
using SantiagoConectaIA.API.EngramaLevels.Infrastructure.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SantiagoConectaIA.API.EngramaLevels.Infrastructure.Repository
{
    public class EmprendimientosFeedRepository : IEmprendimientosFeedRepository
    {
        private readonly IDapperManagerHelper _managerHelper;

        public EmprendimientosFeedRepository(IDapperManagerHelper managerHelper)
        {
            _managerHelper = managerHelper;
        }

        public async Task<IEnumerable<spGetEmprendimientosFeed.Result>> spGetEmprendimientosFeed(spGetEmprendimientosFeed.Request daoModel)
        {
            var respuesta = await _managerHelper.GetAllAsync<spGetEmprendimientosFeed.Result, spGetEmprendimientosFeed.Request>(daoModel, "", "SCIA");
            if (respuesta.Ok)
            {
                return respuesta.Data;
            }
            return new List<spGetEmprendimientosFeed.Result> { new() { bResult = false, vchMessage = respuesta.Msg } };
        }

        public async Task<IEnumerable<spSearchEmprendimientosFeed.Result>> spSearchEmprendimientosFeed(spSearchEmprendimientosFeed.Request daoModel)
        {
            var respuesta = await _managerHelper.GetAllAsync<spSearchEmprendimientosFeed.Result, spSearchEmprendimientosFeed.Request>(daoModel, "", "SCIA");
            if (respuesta.Ok)
            {
                return respuesta.Data;
            }
            return new List<spSearchEmprendimientosFeed.Result> { new() { bResult = false, vchMessage = respuesta.Msg } };
        }
    }
}
