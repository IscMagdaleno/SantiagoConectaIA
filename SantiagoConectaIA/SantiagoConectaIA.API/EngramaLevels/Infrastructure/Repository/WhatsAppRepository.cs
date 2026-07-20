using EngramaCoreStandar.Dapper;

using SantiagoConectaIA.API.EngramaLevels.Infrastructure.Entity.WhatsAppModule;
using SantiagoConectaIA.API.EngramaLevels.Infrastructure.Interfaces;

namespace SantiagoConectaIA.API.EngramaLevels.Infrastructure.Repository
{
	public class WhatsAppRepository : IWhatsAppRepository
	{
		private readonly IDapperManagerHelper _managerHelper;

		public WhatsAppRepository(IDapperManagerHelper managerHelper)
		{
			_managerHelper = managerHelper ?? throw new ArgumentNullException(nameof(managerHelper));
		}

		public async Task<spSaveWhatsAppUser.Result> spSaveWhatsAppUser(spSaveWhatsAppUser.Request request)
		{
			var res = await _managerHelper.GetAsync<spSaveWhatsAppUser.Result, spSaveWhatsAppUser.Request>(request, "", "SCIA");
			if (res.Ok && res.Data != null) return res.Data;
			return new spSaveWhatsAppUser.Result { bResult = false, vchMessage = res.Msg };
		}

		public async Task<spSaveWhatsAppConversation.Result> spSaveWhatsAppConversation(spSaveWhatsAppConversation.Request request)
		{
			var res = await _managerHelper.GetAsync<spSaveWhatsAppConversation.Result, spSaveWhatsAppConversation.Request>(request, "", "SCIA");
			if (res.Ok && res.Data != null) return res.Data;
			return new spSaveWhatsAppConversation.Result { bResult = false, vchMessage = res.Msg };
		}

		public async Task<spSaveWhatsAppMessage.Result> spSaveWhatsAppMessage(spSaveWhatsAppMessage.Request request)
		{
			var res = await _managerHelper.GetAsync<spSaveWhatsAppMessage.Result, spSaveWhatsAppMessage.Request>(request, "", "SCIA");
			if (res.Ok && res.Data != null) return res.Data;
			return new spSaveWhatsAppMessage.Result { bResult = false, vchMessage = res.Msg };
		}

		public async Task<IEnumerable<spGetWhatsAppStats.Result>> spGetWhatsAppStats(spGetWhatsAppStats.Request request)
		{
			var res = await _managerHelper.GetAllAsync<spGetWhatsAppStats.Result, spGetWhatsAppStats.Request>(request, "", "SCIA");
			if (res.Ok) return res.Data;
			return new List<spGetWhatsAppStats.Result> { new() { bResult = false, vchMessage = res.Msg } };
		}

		public async Task<IEnumerable<spGetWhatsAppDailyStats.Result>> spGetWhatsAppDailyStats(spGetWhatsAppDailyStats.Request request)
		{
			var res = await _managerHelper.GetAllAsync<spGetWhatsAppDailyStats.Result, spGetWhatsAppDailyStats.Request>(request, "", "SCIA");
			if (res.Ok) return res.Data;
			return new List<spGetWhatsAppDailyStats.Result> { new() { bResult = false, vchMessage = res.Msg } };
		}
	}
}
