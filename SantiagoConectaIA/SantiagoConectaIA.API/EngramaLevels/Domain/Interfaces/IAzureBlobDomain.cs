using EngramaCoreStandar.Results;

using SantiagoConectaIA.Share.Objects.Common;

namespace SantiagoConectaIA.API.EngramaLevels.Domain.Interfaces
{
	public interface IAzureBlobDomain
	{
		Task<Response<BlobSaved>> UploadDocument(Stream fileStream, string fileName, string containerName);
	}
}
