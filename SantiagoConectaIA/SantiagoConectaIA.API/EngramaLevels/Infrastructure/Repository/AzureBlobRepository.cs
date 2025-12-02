using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Sas;

using EngramaCoreStandar.Results;

using SantiagoConectaIA.API.EngramaLevels.Infrastructure.Interfaces;
using SantiagoConectaIA.Share.Objects.Common;

namespace SantiagoConectaIA.EngramaLevels.API.Infrastructure.Repository
{
	public class AzureBlobRepository : IAzureBlobRepository
	{
		private readonly string _connectionString;
		private readonly IConfiguration _configuration;
		private readonly BlobServiceClient _blobServiceClient;

		public AzureBlobRepository(IConfiguration configuration)
		{
			_configuration = configuration;
			// Obtener la cadena de conexión de Azure desde la configuración (ej. appsettings.json)
			// Asegúrate de que esta clave coincide con la que usas en tu appsettings.json
			_connectionString = _configuration["AzureBlobStorage:ConnectionString"];

			// Inicializar el cliente de servicio
			if (string.IsNullOrEmpty(_connectionString))
			{
				throw new InvalidOperationException("La cadena de conexión de Azure Blob Storage no está configurada.");
			}
			_blobServiceClient = new BlobServiceClient(_connectionString);
		}

		/// <summary>
		/// Sube un archivo a Azure Blob Storage y retorna su URL.
		/// </summary>
		public async Task<Response<BlobSaved>> UploadFileAsync(Stream fileStream, string fileName, string containerName)
		{
			var response = new Response<BlobSaved>();
			response.Data = new BlobSaved();

			try
			{
				// 1. Obtener la referencia al contenedor
				var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);

				// 2. Crear el contenedor si no existe (opcional, pero seguro)
				await containerClient.CreateIfNotExistsAsync(PublicAccessType.Blob);

				// 3. Obtener la referencia al Blob (archivo)
				var blobClient = containerClient.GetBlobClient(fileName);

				// 4. Subir el Stream al Blob, sobrescribiendo si ya existe
				await blobClient.UploadAsync(fileStream, overwrite: true);

				// 5. Generar la URL con token SAS (para visualización segura)
				// Usaremos un token que es válido por un tiempo limitado (ej. 1 hora)
				if (blobClient.CanGenerateSasUri)
				{
					var sasBuilder = new BlobSasBuilder()
					{
						BlobContainerName = containerClient.Name,
						BlobName = blobClient.Name,
						Resource = "b" // "b" para Blob
					};

					// Definir los permisos y el tiempo de expiración
					sasBuilder.ExpiresOn = DateTimeOffset.UtcNow.AddHours(1); // Expira en 1 hora
					sasBuilder.SetPermissions(BlobSasPermissions.Read); // Solo permiso de lectura

					// Generar la URI con el token SAS
					Uri blobSasUri = blobClient.GenerateSasUri(sasBuilder);

					response.Data.URL = blobSasUri.ToString(); // Retorna la URL completa con el token
					response.Data.Name = blobClient.Name;
					response.IsSuccess = true;
					response.Message = "Archivo subido y URL SAS generada con éxito.";
				}
				else
				{
					// Si no se pudo generar el token, retorna la URL base (menos segura)

					response.Data.URL = ""; // Retorna la URL completa con el token
					response.Data.Name = blobClient.Name;
					response.IsSuccess = true;
					response.Message = "Archivo subido. No se pudo generar el token SAS.";
				}
			}
			catch (Exception ex)
			{
				response.IsSuccess = false;
				response.Message = $"Error al subir el archivo a Azure Blob: {ex.Message}";
			}

			return response;
		}
	}

}