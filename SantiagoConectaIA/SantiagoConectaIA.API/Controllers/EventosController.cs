using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SantiagoConectaIA.API.EngramaLevels.Domain.Interfaces;
using SantiagoConectaIA.API.EngramaLevels.Domain.Interfaces.EventosModule;
using SantiagoConectaIA.API.Services;
using SantiagoConectaIA.Share.Objects.Common;
using SantiagoConectaIA.Share.Objects.EventosModulo;
using SantiagoConectaIA.Share.PostClass.EventosModulo;
using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SantiagoConectaIA.API.Controllers
{
    /// <summary>
    /// Controlador para el modulo de Eventos, siguiendo la metodologia Engrama.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class EventosController : ControllerBase
    {
        private readonly IEventosDomain _eventosDomain;
        private readonly IAzureBlobDomain _azureBlobDomain;
        private readonly IGeminiEventoService _geminiEventoService;

        /// <summary>
        /// Inicializa el controlador con sus dependencias.
        /// </summary>
        public EventosController(
            IEventosDomain eventosDomain, 
            IAzureBlobDomain azureBlobDomain, 
            IGeminiEventoService geminiEventoService)
        {
            _eventosDomain = eventosDomain;
            _azureBlobDomain = azureBlobDomain;
            _geminiEventoService = geminiEventoService;
        }

        /// <summary>
        /// Sube el flyer/cartel de un evento a Azure Blob y extrae automáticamente los datos mediante Gemini Vision.
        /// </summary>
        [HttpPost("PostEscanearEventoConIA")]
        public async Task<IActionResult> PostEscanearEventoConIA(IFormFile? image, CancellationToken cancellationToken)
        {
            var uploadedFile = image ?? (Request.HasFormContentType ? Request.Form.Files.FirstOrDefault() : null);
            if (uploadedFile == null || uploadedFile.Length == 0)
            {
                return BadRequest(EngramaCoreStandar.Results.Response<EventoExtraidoDto>.BadResult("No se proporcionó ninguna imagen del evento.", new EventoExtraidoDto()));
            }

            try
            {
                using var stream = uploadedFile.OpenReadStream();
                var formato = await FormatoArchivo.DetectarAsync(stream, cancellationToken);
                if (formato is not { EsImagen: true })
                {
                    return BadRequest(EngramaCoreStandar.Results.Response<EventoExtraidoDto>.BadResult(
                        "El archivo no es una imagen válida (PNG, JPG, WEBP).", new EventoExtraidoDto()));
                }

                stream.Position = 0;
                var uniqueFileName = $"evento_ai_{Guid.NewGuid()}.{formato.Extension}";
                var blobResult = await _azureBlobDomain.UploadDocument(stream, uniqueFileName, "eventos");

                if (!blobResult.IsSuccess || blobResult.Data == null || string.IsNullOrWhiteSpace(blobResult.Data.URL))
                {
                    return BadRequest(EngramaCoreStandar.Results.Response<EventoExtraidoDto>.BadResult(
                        $"Error al subir la imagen al almacenamiento: {blobResult.Message}", new EventoExtraidoDto()));
                }

                stream.Position = 0;
                var aiResult = await _geminiEventoService.ExtraerEventoDesdeImagenAsync(
                    stream, 
                    formato.MimeType, 
                    blobResult.Data.URL, 
                    cancellationToken);

                if (aiResult.IsSuccess)
                {
                    return Ok(aiResult);
                }

                return BadRequest(aiResult);
            }
            catch (Exception ex)
            {
                return BadRequest(EngramaCoreStandar.Results.Response<EventoExtraidoDto>.BadResult(
                    $"Error inesperado al escanear el evento: {ex.Message}", new EventoExtraidoDto()));
            }
        }

        /// <summary>
        /// Consulta la lista de eventos filtrados por ID, categoría, estatus o destacado.
        /// </summary>
        [HttpPost("PostGetEventos")]
        public async Task<IActionResult> PostGetEventos([FromBody] PostGetEventos postModel)
        {
            var result = await _eventosDomain.GetEventos(postModel);
            if (result.IsSuccess)
            {
                return Ok(result);
            }
            return BadRequest(result);
        }

        /// <summary>
        /// Guarda o actualiza un registro en la tabla Eventos.
        /// </summary>
        [HttpPost("PostSaveEvento")]
        public async Task<IActionResult> PostSaveEvento([FromBody] PostSaveEvento postModel)
        {
            var result = await _eventosDomain.SaveEvento(postModel);
            if (result.IsSuccess) return Ok(result);
            return BadRequest(result);
        }

        /// <summary>
        /// Consulta el catálogo de categorías de eventos.
        /// </summary>
        [HttpPost("PostGetCategoriaEventos")]
        public async Task<IActionResult> PostGetCategoriaEventos([FromBody] PostGetCategoriaEventos postModel)
        {
            var result = await _eventosDomain.GetCategoriaEventos(postModel);
            if (result.IsSuccess) return Ok(result);
            return BadRequest(result);
        }

        /// <summary>
        /// Guarda o actualiza una categoría de evento.
        /// </summary>
        [HttpPost("PostSaveCategoriaEvento")]
        public async Task<IActionResult> PostSaveCategoriaEvento([FromBody] PostSaveCategoriaEvento postModel)
        {
            var result = await _eventosDomain.SaveCategoriaEvento(postModel);
            if (result.IsSuccess) return Ok(result);
            return BadRequest(result);
        }

        /// <summary>
        /// Consulta las imágenes asociadas a un registro (genérico).
        /// </summary>
        [HttpPost("PostGetImagenesRegistro")]
        public async Task<IActionResult> PostGetImagenesRegistro([FromBody] PostGetImagenesRegistro postModel)
        {
            var result = await _eventosDomain.GetImagenesRegistro(postModel);
            if (result.IsSuccess) return Ok(result);
            return BadRequest(result);
        }

        /// <summary>
        /// Guarda una imagen para un registro (genérico).
        /// </summary>
        [HttpPost("PostSaveImagenRegistro")]
        public async Task<IActionResult> PostSaveImagenRegistro([FromBody] PostSaveImagenRegistro postModel)
        {
            var result = await _eventosDomain.SaveImagenRegistro(postModel);
            if (result.IsSuccess) return Ok(result);
            return BadRequest(result);
        }

        /// <summary>
        /// Elimina (desactiva) una imagen de registro (genérico).
        /// </summary>
        [HttpPost("PostDeleteImagenRegistro")]
        public async Task<IActionResult> PostDeleteImagenRegistro([FromBody] PostDeleteImagenRegistro postModel)
        {
            var result = await _eventosDomain.DeleteImagenRegistro(postModel);
            if (result.IsSuccess) return Ok(result);
            return BadRequest(result);
        }

        /// <summary>
        /// Consulta el detalle completo de un evento incluyendo todas sus imágenes relacionadas.
        /// </summary>
        [HttpPost("PostGetEventoDetalle")]
        public async Task<IActionResult> PostGetEventoDetalle([FromBody] PostGetEventoDetalle postModel)
        {
            var result = await _eventosDomain.GetEventoDetalle(postModel);
            if (result.IsSuccess) return Ok(result);
            return BadRequest(result);
        }

        /// <summary>
        /// Consulta las sucursales/locales asociadas a un evento.
        /// </summary>
        [HttpPost("PostGetEventosSucursales")]
        public async Task<IActionResult> PostGetEventosSucursales([FromBody] PostGetEventosSucursales postModel)
        {
            var result = await _eventosDomain.GetEventosSucursales(postModel);
            if (result.IsSuccess) return Ok(result);
            return BadRequest(result);
        }

        /// <summary>
        /// Guarda o actualiza una sucursal/local de evento.
        /// </summary>
        [HttpPost("PostSaveSucursalEvento")]
        public async Task<IActionResult> PostSaveSucursalEvento([FromBody] PostSaveSucursalEvento postModel)
        {
            var result = await _eventosDomain.SaveSucursalEvento(postModel);
            if (result.IsSuccess) return Ok(result);
            return BadRequest(result);
        }
    }
}
