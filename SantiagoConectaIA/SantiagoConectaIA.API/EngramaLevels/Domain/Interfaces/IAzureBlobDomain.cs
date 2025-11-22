using EngramaCoreStandar.Results;

namespace SantiagoConectaIA.API.EngramaLevels.Domain.Interfaces
{
	public interface IAzureBlobDomain
	{
		Task<Response<string>> UploadDocument(Stream fileStream, string fileName, string containerName);
	}
}
