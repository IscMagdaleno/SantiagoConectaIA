using EngramaCoreStandar.Results;

using SantiagoConectaIA.API.EngramaLevels.Domain.Interfaces;
using SantiagoConectaIA.API.EngramaLevels.Infrastructure.Interfaces;
using SantiagoConectaIA.Share.Objects.Common;

namespace SantiagoConectaIA.API.EngramaLevels.Domain.Core
{
	public class AzureBlobDomain : IAzureBlobDomain
	{
		private readonly IAzureBlobRepository _blobRepository;

		public AzureBlobDomain(IAzureBlobRepository blobRepository)
		{
			_blobRepository = blobRepository;
		}

		public async Task<Response<BlobSaved>> UploadDocument(Stream fileStream, string fileName, string containerName)
		{
			if (fileStream == null || string.IsNullOrEmpty(fileName))
			{
				return Response<BlobSaved>.BadResult("Datos del archivo no válidos.", new BlobSaved());
			}

			// Llama al repositorio para la subida
			var result = await _blobRepository.UploadFileAsync(fileStream, fileName, containerName);

			return result;
		}


	}
}
