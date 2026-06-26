using System.Collections.Generic;
using System.Threading.Tasks;
using EngramaCoreStandar.Dapper;
using EngramaCoreStandar.Dapper.Interfaces;
using SantiagoConectaIA.API.EngramaLevels.Infrastructure.Entity.PageVisitsModule;
using SantiagoConectaIA.API.EngramaLevels.Infrastructure.Interfaces;

namespace SantiagoConectaIA.API.EngramaLevels.Infrastructure.Repository
{
	public class PageVisitsRepository : IPageVisitsRepository
	{
		private readonly IDapperManagerHelper _managerHelper;

		public PageVisitsRepository(IDapperManagerHelper managerHelper)
		{
			_managerHelper = managerHelper;
		}

		public async Task<IEnumerable<spGetPageVisitsStats.Result>> spGetPageVisitsStats(spGetPageVisitsStats.Request daoModel)
		{
			var respuesta = await _managerHelper.GetAllAsync<spGetPageVisitsStats.Result, spGetPageVisitsStats.Request>(daoModel, "", "SCIA");
			if (respuesta.Ok)
			{
				return respuesta.Data;
			}
			return new List<spGetPageVisitsStats.Result> { new() { bResult = false, vchMessage = respuesta.Msg } };
		}
	}
}
