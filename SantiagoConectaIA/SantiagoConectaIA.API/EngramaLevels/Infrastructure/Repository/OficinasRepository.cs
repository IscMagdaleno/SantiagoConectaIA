using EngramaCoreStandar.Dapper;

using SantiagoConectaIA.API.EngramaLevels.Infrastructure.Entity.OficinasModule;
using SantiagoConectaIA.API.EngramaLevels.Infrastructure.Interfaces;
// File: OficinasRepository.cs

namespace SantiagoConectaIA.API.EngramaLevels.Infrastructure.Repository
{
	public class OficinasRepository : IOficinasRepository
	{
		private readonly IDapperManagerHelper _managerHelper;

		public OficinasRepository(IDapperManagerHelper managerHelper)
		{
			_managerHelper = managerHelper ?? throw new ArgumentNullException(nameof(managerHelper));
		}

		public async Task<IEnumerable<spGetOficinas.Result>> spGetOficinas(spGetOficinas.Request request)
		{
			var res = await _managerHelper.GetAllAsync<spGetOficinas.Result, spGetOficinas.Request>(request, "");
			if (res.Ok) return res.Data;
			return new List<spGetOficinas.Result> { new spGetOficinas.Result { bResult = false, vchMessage = res.Msg } };
		}

		public async Task<spSearchOficinas.Result[]> spSearchOficinas(spSearchOficinas.Request request)
		{
			var res = await _managerHelper.GetAllAsync<spSearchOficinas.Result, spSearchOficinas.Request>(request, "");
			if (res.Ok) return res.Data?.ToArray() ?? Array.Empty<spSearchOficinas.Result>();
			return new spSearchOficinas.Result[] { new spSearchOficinas.Result { bResult = false, vchMessage = res.Msg } };
		}

		public async Task<spSaveOficina.Result> spSaveOficina(spSaveOficina.Request request)
		{
			var res = await _managerHelper.GetAsync<spSaveOficina.Result, spSaveOficina.Request>(request, "");
			if (res.Ok && res.Data != null) return res.Data;
			return new spSaveOficina.Result { bResult = false, vchMessage = res.Msg };
		}

		public async Task<IEnumerable<spGetDependencias.Result>> spGetDependencias(spGetDependencias.Request request)
		{
			var res = await _managerHelper.GetAllAsync<spGetDependencias.Result, spGetDependencias.Request>(request, "");
			if (res.Ok) return res.Data;
			return new List<spGetDependencias.Result> { new spGetDependencias.Result { bResult = false, vchMessage = res.Msg } };
		}

		public async Task<spSaveDependencia.Result> spSaveDependencia(spSaveDependencia.Request request)
		{
			var res = await _managerHelper.GetAsync<spSaveDependencia.Result, spSaveDependencia.Request>(request, "");
			if (res.Ok && res.Data != null) return res.Data;
			return new spSaveDependencia.Result { bResult = false, vchMessage = res.Msg };
		}

		public async Task<spLinkOficinaTramite.Result> spLinkOficinaTramite(spLinkOficinaTramite.Request request)
		{
			var res = await _managerHelper.GetAsync<spLinkOficinaTramite.Result, spLinkOficinaTramite.Request>(request, "");
			if (res.Ok && res.Data != null) return res.Data;
			return new spLinkOficinaTramite.Result { bResult = false, vchMessage = res.Msg };
		}

		public async Task<IEnumerable<spGetOficinasPorTramite.Result>> spGetOficinasPorTramite(spGetOficinasPorTramite.Request request)
		{
			var res = await _managerHelper.GetAllAsync<spGetOficinasPorTramite.Result, spGetOficinasPorTramite.Request>(request, "");
			if (res.Ok) return res.Data;
			return new List<spGetOficinasPorTramite.Result> { new spGetOficinasPorTramite.Result { bResult = false, vchMessage = res.Msg } };
		}
	}

}
