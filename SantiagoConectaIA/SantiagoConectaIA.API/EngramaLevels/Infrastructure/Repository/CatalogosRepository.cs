using System.Collections.Generic;
using System.Threading.Tasks;
using EngramaCoreStandar.Dapper;
using SantiagoConectaIA.API.EngramaLevels.Infrastructure.Entity.CatalogosModule;
using SantiagoConectaIA.API.EngramaLevels.Infrastructure.Interfaces;

namespace SantiagoConectaIA.API.EngramaLevels.Infrastructure.Repository
{
    public class CatalogosRepository : ICatalogosRepository
    {
        private readonly IDapperManagerHelper _managerHelper;

        public CatalogosRepository(IDapperManagerHelper managerHelper)
        {
            _managerHelper = managerHelper;
        }

        public async Task<IEnumerable<spGetCatalogos.Result>> spGetCatalogos(spGetCatalogos.Request request)
        {
            var res = await _managerHelper.GetAllAsync<spGetCatalogos.Result, spGetCatalogos.Request>(request, "", "SCIA");
            if (res.Ok) return res.Data;
            return new List<spGetCatalogos.Result> { new spGetCatalogos.Result { bResult = false, vchMessage = res.Msg } };
        }

        public async Task<spGetParametro.Result> spGetParametro(spGetParametro.Request request)
        {
            var res = await _managerHelper.GetAsync<spGetParametro.Result, spGetParametro.Request>(request, "", "SCIA");
            if (res.Ok && res.Data != null) return res.Data;
            return new spGetParametro.Result { bResult = false, vchMessage = res.Msg };
        }
    }
}
