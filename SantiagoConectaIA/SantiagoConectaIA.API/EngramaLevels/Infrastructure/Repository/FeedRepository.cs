using EngramaCoreStandar.Dapper;
using SantiagoConectaIA.API.EngramaLevels.Infrastructure.Entity.FeedModule;
using SantiagoConectaIA.API.EngramaLevels.Infrastructure.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SantiagoConectaIA.API.EngramaLevels.Infrastructure.Repository
{
    public class FeedRepository : IFeedRepository
    {
        private readonly IDapperManagerHelper _managerHelper;

        public FeedRepository(IDapperManagerHelper managerHelper)
        {
            _managerHelper = managerHelper;
        }

        public async Task<IEnumerable<spGetFeed.Result>> spGetFeed(spGetFeed.Request daoModel)
        {
            var respuesta = await _managerHelper.GetAllAsync<spGetFeed.Result, spGetFeed.Request>(daoModel, "", "SCIA");
            if (respuesta.Ok)
            {
                return respuesta.Data;
            }
            return new List<spGetFeed.Result> { new() { bResult = false, vchMessage = respuesta.Msg } };
        }

        public async Task<IEnumerable<spSearchFeed.Result>> spSearchFeed(spSearchFeed.Request daoModel)
        {
            var respuesta = await _managerHelper.GetAllAsync<spSearchFeed.Result, spSearchFeed.Request>(daoModel, "", "SCIA");
            if (respuesta.Ok)
            {
                return respuesta.Data;
            }
            return new List<spSearchFeed.Result> { new() { bResult = false, vchMessage = respuesta.Msg } };
        }
    }
}
