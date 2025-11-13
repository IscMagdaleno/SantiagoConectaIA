using EngramaCoreStandar.Dapper;

using SantiagoConectaIA.API.EngramaLevels.Infrastructure.Entity.TramitesModule;
using SantiagoConectaIA.API.EngramaLevels.Infrastructure.Interfaces;

namespace SantiagoConectaIA.API.EngramaLevels.Infrastructure.Repository
{
	public class TramitesRepository : ITramitesRepository
	{
		private readonly IDapperManagerHelper _managerHelper;

		public TramitesRepository(IDapperManagerHelper managerHelper)
		{
			_managerHelper = managerHelper;
		}

		public async Task<IEnumerable<spGetTramites.Result>> spGetTramites(spGetTramites.Request request)
		{
			var res = await _managerHelper.GetAllAsync<spGetTramites.Result, spGetTramites.Request>(request, "");
			if (res.Ok) return res.Data;
			return new List<spGetTramites.Result> { new spGetTramites.Result { bResult = false, vchMessage = res.Msg } };
		}

		public async Task<IEnumerable<spSearchTramites.Result>> spSearchTramites(spSearchTramites.Request request)
		{
			var res = await _managerHelper.GetAllAsync<spSearchTramites.Result, spSearchTramites.Request>(request, "");
			if (res.Ok) return res.Data;
			return new List<spSearchTramites.Result> { new spSearchTramites.Result { bResult = false, vchMessage = res.Msg } };
		}

		public async Task<spSaveTramite.Result> spSaveTramite(spSaveTramite.Request request)
		{
			var res = await _managerHelper.GetAsync<spSaveTramite.Result, spSaveTramite.Request>(request, "");
			if (res.Ok && res.Data != null) return res.Data;
			return new spSaveTramite.Result { bResult = false, vchMessage = res.Msg };
		}

		public async Task<IEnumerable<spGetRequisitosPorTramite.Result>> spGetRequisitosPorTramite(spGetRequisitosPorTramite.Request request)
		{
			var res = await _managerHelper.GetAllAsync<spGetRequisitosPorTramite.Result, spGetRequisitosPorTramite.Request>(request, "");
			if (res.Ok) return res.Data;
			return new List<spGetRequisitosPorTramite.Result> { new spGetRequisitosPorTramite.Result { bResult = false, vchMessage = res.Msg } };
		}

		public async Task<spSaveRequisito.Result> spSaveRequisito(spSaveRequisito.Request request)
		{
			var res = await _managerHelper.GetAsync<spSaveRequisito.Result, spSaveRequisito.Request>(request, "");
			if (res.Ok && res.Data != null) return res.Data;
			return new spSaveRequisito.Result { bResult = false, vchMessage = res.Msg };
		}

		public async Task<spSaveDocumento.Result> spSaveDocumento(spSaveDocumento.Request request)
		{
			var res = await _managerHelper.GetAsync<spSaveDocumento.Result, spSaveDocumento.Request>(request, "");
			if (res.Ok && res.Data != null) return res.Data;
			return new spSaveDocumento.Result { bResult = false, vchMessage = res.Msg };
		}
		public async Task<IEnumerable<spGetTramitesCard.Result>> spGetTramitesCard(spGetTramitesCard.Request daoModel)
		{
			var respuesta = await _managerHelper.GetAllAsync<spGetTramitesCard.Result, spGetTramitesCard.Request>(daoModel, "");
			if (respuesta.Ok)
			{
				return respuesta.Data;
			}
			return new List<spGetTramitesCard.Result>
			{
				new() { bResult = false, vchMessage = respuesta.Msg }
			};
		}
	}

}
