using EngramaCoreStandar.Dapper;

using SantiagoConectaIA.API.EngramaLevels.Infrastructure.Entity.PublicacionesRegistroModule;
using SantiagoConectaIA.API.EngramaLevels.Infrastructure.Interfaces;

namespace SantiagoConectaIA.API.EngramaLevels.Infrastructure.Repository
{
	public class PublicacionesRegistroRepository : IPublicacionesRegistroRepository
	{
		private readonly IDapperManagerHelper _managerHelper;

		public PublicacionesRegistroRepository(IDapperManagerHelper managerHelper)
		{
			_managerHelper = managerHelper;
		}

		public async Task<spSavePublicacionRegistro.Result> spSavePublicacionRegistro(spSavePublicacionRegistro.Request request)
		{
			var respuesta = await _managerHelper.GetAsync<spSavePublicacionRegistro.Result, spSavePublicacionRegistro.Request>(request, "", "SCIA");
			if (respuesta.Ok && respuesta.Data != null)
			{
				return respuesta.Data;
			}
			return new spSavePublicacionRegistro.Result { bResult = false, vchMessage = respuesta.Msg };
		}

		public async Task<IEnumerable<spGetPublicacionesRegistro.Result>> spGetPublicacionesRegistro(spGetPublicacionesRegistro.Request request)
		{
			var respuesta = await _managerHelper.GetAllAsync<spGetPublicacionesRegistro.Result, spGetPublicacionesRegistro.Request>(request, "", "SCIA");
			if (respuesta.Ok)
			{
				return respuesta.Data;
			}
			return new List<spGetPublicacionesRegistro.Result> { new() { bResult = false, vchMessage = respuesta.Msg } };
		}
	}
}
