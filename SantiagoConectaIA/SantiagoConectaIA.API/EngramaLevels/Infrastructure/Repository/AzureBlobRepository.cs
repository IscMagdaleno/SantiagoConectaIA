using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Sas;

using EngramaCoreStandar.Results;

using Microsoft.AspNetCore.StaticFiles;

using SantiagoConectaIA.API.EngramaLevels.Infrastructure.Interfaces;
using SantiagoConectaIA.API.Services;
using SantiagoConectaIA.Share.Objects.Common;

namespace SantiagoConectaIA.EngramaLevels.API.Infrastructure.Repository
{
	public class AzureBlobRepository : IAzureBlobRepository
	{
		private static readonly FileExtensionContentTypeProvider ContentTypes = new()
		{
			Mappings =
			{
				[".heic"] = "image/heic",
				[".heif"] = "image/heif",
				[".webp"] = "image/webp"
			}
		};

		private readonly IParametrosService _parametros;

		public AzureBlobRepository(IParametrosService parametros)
		{
			_parametros = parametros;
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
				var connectionString = await _parametros.GetValor1Async(ParametrosAlias.AzureBlobStorage);
				if (string.IsNullOrEmpty(connectionString))
				{
					response.IsSuccess = false;
					response.Message = $"La cadena de conexión de Azure Blob Storage no está configurada (parámetro {ParametrosAlias.AzureBlobStorage}).";
					return response;
				}

				// 1. Obtener la referencia al contenedor
				var containerClient = new BlobServiceClient(connectionString).GetBlobContainerClient(containerName);

				// 2. Crear el contenedor si no existe (opcional, pero seguro)
				await containerClient.CreateIfNotExistsAsync(PublicAccessType.Blob);

				// 3. Obtener la referencia al Blob (archivo)
				var blobClient = containerClient.GetBlobClient(fileName);

				// 4. Subir el Stream al Blob con su Content-Type, sobrescribiendo si ya existe
				if (!ContentTypes.TryGetContentType(fileName, out var contentType))
				{
					contentType = "application/octet-stream";
				}

				await blobClient.UploadAsync(fileStream, new BlobUploadOptions
				{
					HttpHeaders = new BlobHttpHeaders { ContentType = contentType }
				});

				// 5. Generar la URL con token SAS (para visualización segura)
				// Usaremos un token que es válido por un tiempo limitado (ej. Expira en 10  años)
				if (blobClient.CanGenerateSasUri)
				{
					var sasBuilder = new BlobSasBuilder()
					{
						BlobContainerName = containerClient.Name,
						BlobName = blobClient.Name,
						Resource = "b" // "b" para Blob
					};


					// Definir los permisos y el tiempo de expiración
					sasBuilder.ExpiresOn = DateTimeOffset.UtcNow.AddYears(10); // Expira en 10  años
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