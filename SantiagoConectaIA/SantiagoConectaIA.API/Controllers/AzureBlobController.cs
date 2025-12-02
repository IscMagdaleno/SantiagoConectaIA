using Microsoft.AspNetCore.Mvc;

using SantiagoConectaIA.API.EngramaLevels.Domain.Interfaces;
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
		public async Task<IActionResult> UploadDocument(IFormFile file)
		{
			if (file == null || file.Length == 0)
			{
				return BadRequest(EngramaCoreStandar.Results.Response<BlobSaved>.BadResult("No se proporcionó ningún archivo.", new BlobSaved()));
			}


			// Generar un nombre único para el archivo
			var extension = Path.GetExtension(file.FileName);
			var uniqueFileName = $"{Guid.NewGuid()}{extension}";

			// Usar 'using' para asegurar que el stream se cierra correctamente
			using (var stream = file.OpenReadStream())
			{
				var result = await _azureBlobDomain.UploadDocument(stream, uniqueFileName, "tramitedocs");

				if (result.IsSuccess)
				{
					return Ok(result);
				}
				return BadRequest(result);
			}
		}
	}
}