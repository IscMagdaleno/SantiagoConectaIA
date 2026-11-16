using EngramaCoreStandar.Dapper;

using SantiagoConectaIA.API.EngramaLevels.Infrastructure.Entity.MemesModule;
using SantiagoConectaIA.API.EngramaLevels.Infrastructure.Interfaces;

namespace SantiagoConectaIA.API.EngramaLevels.Infrastructure.Repository
{
	public class MemesRepository : IMemesRepository
	{
		private readonly IDapperManagerHelper _managerHelper;

		public MemesRepository(IDapperManagerHelper managerHelper)
		{
			_managerHelper = managerHelper;
		}

		public async Task<spSaveMemeCreado.Result> spSaveMemeCreado(spSaveMemeCreado.Request request)
		{
			var respuesta = await _managerHelper.GetAsync<spSaveMemeCreado.Result, spSaveMemeCreado.Request>(request, "", "SCIA");
			if (respuesta.Ok && respuesta.Data != null)
			{
				return respuesta.Data;
			}
			return new spSaveMemeCreado.Result { bResult = false, vchMessage = respuesta.Msg };
		}

		public async Task<IEnumerable<spGetMemesCreados.Result>> spGetMemesCreados(spGetMemesCreados.Request request)
		{
			var respuesta = await _managerHelper.GetAllAsync<spGetMemesCreados.Result, spGetMemesCreados.Request>(request, "", "SCIA");
			if (respuesta.Ok)
			{
				return respuesta.Data;
			}
			return new List<spGetMemesCreados.Result> { new() { bResult = false, vchMessage = respuesta.Msg } };
		}

		public async Task<spGetMemesResumen.Result> spGetMemesResumen(spGetMemesResumen.Request request)
		{
			var respuesta = await _managerHelper.GetAsync<spGetMemesResumen.Result, spGetMemesResumen.Request>(request, "", "SCIA");
			if (respuesta.Ok && respuesta.Data != null)
			{
				return respuesta.Data;
			}
			return new spGetMemesResumen.Result { bResult = false, vchMessage = respuesta.Msg };
		}

		public async Task<spDescartarMeme.Result> spDescartarMeme(spDescartarMeme.Request request)
		{
			var respuesta = await _managerHelper.GetAsync<spDescartarMeme.Result, spDescartarMeme.Request>(request, "", "SCIA");
			if (respuesta.Ok && respuesta.Data != null)
			{
				return respuesta.Data;
			}
			return new spDescartarMeme.Result { bResult = false, vchMessage = respuesta.Msg };
		}
	}
}
