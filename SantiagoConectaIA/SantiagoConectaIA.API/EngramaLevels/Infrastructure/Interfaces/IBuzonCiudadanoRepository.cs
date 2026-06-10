using System.Threading.Tasks;
using SantiagoConectaIA.API.EngramaLevels.Infrastructure.Entity.BuzonCiudadanoModule;

namespace SantiagoConectaIA.API.EngramaLevels.Infrastructure.Interfaces
{
	public interface IBuzonCiudadanoRepository
	{
		Task<spSaveBuzonCiudadano.Result> spSaveBuzonCiudadano(spSaveBuzonCiudadano.Request daoModel);
	}
}
