using SantiagoConectaIA.API.EngramaLevels.Infrastructure.Entity.LogsModule;

namespace SantiagoConectaIA.API.EngramaLevels.Infrastructure.Interfaces
{
	public interface ILogsRepository
	{
		Task<spSaveApiCallLog.Result> spSaveApiCallLog(spSaveApiCallLog.Request request);
	}
}
