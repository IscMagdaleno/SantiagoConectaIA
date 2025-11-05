using EngramaCoreStandar.Dapper;

using SantiagoConectaIA.API.EngramaLevels.Infrastructure.Entity.ConversationalModule;
using SantiagoConectaIA.API.EngramaLevels.Infrastructure.Interfaces;

namespace SantiagoConectaIA.API.EngramaLevels.Infrastructure.Repository
{


	public class ConversationalRepository : IConversationalRepository
	{
		private readonly IDapperManagerHelper _managerHelper;

		public ConversationalRepository(IDapperManagerHelper managerHelper)
		{
			_managerHelper = managerHelper ?? throw new ArgumentNullException(nameof(managerHelper));
		}

		public async Task<IEnumerable<spSearchTramitesForChat.Result>> spSearchTramitesForChat(spSearchTramitesForChat.Request req)
		{
			var res = await _managerHelper.GetAllAsync<spSearchTramitesForChat.Result, spSearchTramitesForChat.Request>(req, "");
			if (res.Ok) return res.Data;
			return new List<spSearchTramitesForChat.Result> { new spSearchTramitesForChat.Result { bResult = false, vchMessage = res.Msg } };
		}

		public async Task<IEnumerable<spSearchOficinasForChat.Result>> spSearchOficinasForChat(spSearchOficinasForChat.Request req)
		{
			var res = await _managerHelper.GetAllAsync<spSearchOficinasForChat.Result, spSearchOficinasForChat.Request>(req, "");
			if (res.Ok) return res.Data;
			return new List<spSearchOficinasForChat.Result> { new spSearchOficinasForChat.Result { bResult = false, vchMessage = res.Msg } };
		}


		public async Task<IEnumerable<spSearchRequisitosForChat.Result>> spSearchRequisitosForChat(spSearchRequisitosForChat.Request req)
		{
			var res = await _managerHelper.GetAllAsync<spSearchRequisitosForChat.Result, spSearchRequisitosForChat.Request>(req, "");
			if (res.Ok) return res.Data;
			return new List<spSearchRequisitosForChat.Result> { new spSearchRequisitosForChat.Result { bResult = false, vchMessage = res.Msg } };
		}

		public async Task<IEnumerable<spSearchCostoForChat.Result>> spSearchCostoForChat(spSearchCostoForChat.Request req)
		{
			var res = await _managerHelper.GetAllAsync<spSearchCostoForChat.Result, spSearchCostoForChat.Request>(req, "");
			if (res.Ok) return res.Data;
			// La capa de Infraestructura devuelve el mensaje de error del SP o del DapperManagerHelper
			return new List<spSearchCostoForChat.Result> { new spSearchCostoForChat.Result { bResult = false, vchMessage = res.Msg } };
		}

		public async Task<IEnumerable<spSearchOficinasByTramite.Result>> spSearchOficinasByTramite(spSearchOficinasByTramite.Request req)
		{
			var res = await _managerHelper.GetAllAsync<spSearchOficinasByTramite.Result, spSearchOficinasByTramite.Request>(req, "");
			if (res.Ok) return res.Data;
			// La capa de Infraestructura devuelve el mensaje de error del SP o del DapperManagerHelper
			return new List<spSearchOficinasByTramite.Result> { new spSearchOficinasByTramite.Result { bResult = false, vchMessage = res.Msg } };
		}
	}

}
