using Microsoft.AspNetCore.Mvc;

using SantiagoConectaIA.API.EngramaLevels.Domain.Interfaces;
using SantiagoConectaIA.API.Services;
using SantiagoConectaIA.Share.Objects.Common;

namespace SantiagoConectaIA.API.Controllers
{
	[ApiController]
	[Route("api/[controller]")]
	public class AzureBlobController : ControllerBase
	{
		private readonly IAzureBlobDomain _azureBlobDomain;

		public AzureBlobController(IAzureBlobDomain azureBlobDomain)
		{
			_azureBlobDomain = azureBlobDomain;
		}

		/// <summary>
		/// Sube un archivo PDF al Azure Blob Storage y retorna su URL.
		/// </summary>
		/// <param name="file">El archivo subido.</param>
		[HttpPost("UploadDocument")]
		public async Task<IActionResult> UploadDocument(IFormFile? file)
		{
			var uploadedFile = file ?? (Request.HasFormContentType ? Request.Form.Files.FirstOrDefault() : null);

			if (uploadedFile == null || uploadedFile.Length == 0)
			{
				return BadRequest(EngramaCoreStandar.Results.Response<BlobSaved>.BadResult("No se proporcionó ningún archivo.", new BlobSaved()));
			}

			using (var stream = uploadedFile.OpenReadStream())
			{
				var formato = await FormatoArchivo.DetectarAsync(stream, HttpContext.RequestAborted);
				var extension = formato != null ? $".{formato.Extension}" : Path.GetExtension(uploadedFile.FileName);
				var uniqueFileName = $"{Guid.NewGuid()}{extension}";

				var result = await _azureBlobDomain.UploadDocument(stream, uniqueFileName, "tramitedocs");

				if (result.IsSuccess)
				{
					return Ok(result);
				}
				return BadRequest(result);
			}
		}

		/// <summary>
		/// Sube un archivo de imagen (Logo) al Azure Blob Storage y retorna su URL.
		/// </summary>
		/// <param name="image">El archivo de imagen subido.</param>
		[HttpPost("UploadImage-empresas")]
		public Task<IActionResult> UploadImageEmpresas(IFormFile image) => SubirImagen(image, "empresas");

		/// <summary>
		/// Sube un archivo de imagen de Eventos al Azure Blob Storage y retorna su URL.
		/// </summary>
		/// <param name="image">El archivo de imagen subido.</param>
		[HttpPost("UploadImage-Eventos")]
		public Task<IActionResult> UploadImageEventos(IFormFile image) => SubirImagen(image, "eventos");

		/// <summary>
		/// Sube un archivo de imagen (Portada) de Noticias al Azure Blob Storage y retorna su URL.
		/// </summary>
		/// <param name="image">El archivo de imagen subido.</param>
		[HttpPost("UploadImage-noticias")]
		public Task<IActionResult> UploadImageNoticias(IFormFile image) => SubirImagen(image, "noticias");

		private async Task<IActionResult> SubirImagen(IFormFile? image, string contenedor)
		{
			if (image == null || image.Length == 0)
			{
				return BadRequest(EngramaCoreStandar.Results.Response<BlobSaved>.BadResult("No se proporcionó ninguna imagen.", new BlobSaved()));
			}

			using (var stream = image.OpenReadStream())
			{
				var formato = await FormatoArchivo.DetectarAsync(stream, HttpContext.RequestAborted);
				if (formato is not { EsImagen: true })
				{
					return BadRequest(EngramaCoreStandar.Results.Response<BlobSaved>.BadResult(
						"El archivo no es una imagen PNG, JPG, WEBP, GIF o HEIC.", new BlobSaved()));
				}

				var uniqueFileName = $"{Guid.NewGuid()}.{formato.Extension}";
				var result = await _azureBlobDomain.UploadDocument(stream, uniqueFileName, contenedor);

				if (result.IsSuccess)
				{
					return Ok(result);
				}
				return BadRequest(result);
			}
		}
	}
}
