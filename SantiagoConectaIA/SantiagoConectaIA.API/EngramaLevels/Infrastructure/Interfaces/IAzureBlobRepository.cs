using EngramaCoreStandar.Results;

using SantiagoConectaIA.Share.Objects.Common;

namespace SantiagoConectaIA.API.EngramaLevels.Infrastructure.Interfaces
{
	public interface IAzureBlobRepository
	{
		Task<Response<BlobSaved>> UploadFileAsync(Stream fileStream, string fileName, string containerName);
	}
}
