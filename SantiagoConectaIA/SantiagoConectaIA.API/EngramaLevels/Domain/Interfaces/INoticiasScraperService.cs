namespace SantiagoConectaIA.API.EngramaLevels.Domain.Interfaces
{
	public interface INoticiasScraperService
	{
		Task RunScrapingAsync(CancellationToken cancellationToken = default);
	}
}
