using System.Threading.Tasks;
using EngramaCoreStandar.Results;
using SantiagoConectaIA.Share.Objects.BuzonCiudadanoModule;
using SantiagoConectaIA.Share.PostModels.BuzonCiudadanoModule;

namespace SantiagoConectaIA.API.EngramaLevels.Domain.Interfaces
{
	public interface IBuzonCiudadanoDomain
	{
		Task<Response<BuzonCiudadano>> RegistrarReporte(PostSaveBuzonCiudadano postModel);
	}
}
