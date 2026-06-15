using System;
using System.Threading.Tasks;
using EngramaCoreStandar.Dapper;
using SantiagoConectaIA.API.EngramaLevels.Infrastructure.Entity.BuzonCiudadanoModule;
using SantiagoConectaIA.API.EngramaLevels.Infrastructure.Interfaces;

namespace SantiagoConectaIA.API.EngramaLevels.Infrastructure.Repository
{
	public class BuzonCiudadanoRepository : IBuzonCiudadanoRepository
	{
		private readonly IDapperManagerHelper _managerHelper;

		public BuzonCiudadanoRepository(IDapperManagerHelper managerHelper)
		{
			_managerHelper = managerHelper ?? throw new ArgumentNullException(nameof(managerHelper));
		}

		public async Task<spSaveBuzonCiudadano.Result> spSaveBuzonCiudadano(spSaveBuzonCiudadano.Request daoModel)
		{
			var respuesta = await _managerHelper.GetAsync<spSaveBuzonCiudadano.Result, spSaveBuzonCiudadano.Request>(daoModel, "","SCIA");
			if (respuesta.Ok)
			{
				return respuesta.Data;
			}
			return new spSaveBuzonCiudadano.Result { bResult = false, vchMessage = respuesta.Msg };
		}
	}
}
