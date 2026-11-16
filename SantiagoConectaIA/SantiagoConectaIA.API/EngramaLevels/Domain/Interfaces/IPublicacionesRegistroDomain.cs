using EngramaCoreStandar.Results;

using SantiagoConectaIA.Share.Objects.PublicacionesModule;
using SantiagoConectaIA.Share.PostModels.PublicacionesModule;

namespace SantiagoConectaIA.API.EngramaLevels.Domain.Interfaces
{
	public interface IPublicacionesRegistroDomain
	{
		Task<Response<PublicacionRegistro>> SavePublicacionRegistro(PostSavePublicacionRegistro postModel);
		Task<Response<IEnumerable<PublicacionRegistro>>> GetPublicacionesRegistro(PostGetPublicacionesRegistro postModel);
	}
}
