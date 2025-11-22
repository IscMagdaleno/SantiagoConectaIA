using EngramaCoreStandar.Results;

namespace SantiagoConectaIA.API.EngramaLevels.Infrastructure.Interfaces
{
	public interface IAzureBlobRepository
	{
		Task<Response<string>> UploadFileAsync(Stream fileStream, string fileName, string containerName);
	}
}
