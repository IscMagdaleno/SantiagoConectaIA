using SantiagoConectaIA.API.EngramaLevels.Infrastructure.Entity.AuthModule;

namespace SantiagoConectaIA.API.EngramaLevels.Infrastructure.Interfaces.AuthModule
{
	public interface IAuthRepository
	{
		Task<spGetUsuarioAuth.Result> spGetUsuarioAuth(spGetUsuarioAuth.Request request);
		Task<spSaveUsuario.Result> spSaveUsuario(spSaveUsuario.Request request);
	}
}
