using Microsoft.AspNetCore.Mvc;
using SantiagoConectaIA.API.EngramaLevels.Domain.Interfaces;
using SantiagoConectaIA.Share.PostModels.CatalogosModule;
using SantiagoConectaIA.Share.Objects.NoticiasModule;
using EngramaCoreStandar.Results;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SantiagoConectaIA.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CatalogosController : ControllerBase
    {
        private readonly ICatalogosDomain _catalogosDomain;

        public CatalogosController(ICatalogosDomain catalogosDomain)
        {
            _catalogosDomain = catalogosDomain;
        }

        /// <summary>
        /// Obtiene el listado de tipos de datos para metadatos.
        /// </summary>
        [HttpPost("PostGetTipoDatos")]
        public async Task<IActionResult> PostGetTipoDatos()
        {
            var postModel = new PostGetCatalogos { vchGroupAlias = "tipos.datos.noticias" };
            var result = await _catalogosDomain.GetCatalogos(postModel);
            if (!result.IsSuccess)
            {
                return BadRequest(result);
            }

            // Mapeo manual a DTO
            var dtos = result.Data.Select(x => new TipoDatoDto
            {
                iIdTipoDato = int.TryParse(x.Valor, out var id) ? id : x.IdCatalogo,
                nvchTipo = x.Descripcion,
                ncvhDescripcion = x.Descripcion
            }).ToList();

            var response = new Response<List<TipoDatoDto>>
            {
                IsSuccess = true,
                Data = dtos,
                Message = "Tipos de datos cargados exitosamente."
            };
            return Ok(response);
        }

        /// <summary>
        /// Obtiene los catálogos de un grupo por su alias.
        /// </summary>
        [HttpPost("PostGetCatalogos")]
        public async Task<IActionResult> PostGetCatalogos([FromBody] PostGetCatalogos postModel)
        {
            var result = await _catalogosDomain.GetCatalogos(postModel);
            if (result.IsSuccess)
            {
                return Ok(result);
            }
            return BadRequest(result);
        }

        /// <summary>
        /// Obtiene un parámetro por su alias.
        /// </summary>
        [HttpPost("PostGetParametro")]
        public async Task<IActionResult> PostGetParametro([FromBody] PostGetParametro postModel)
        {
            var result = await _catalogosDomain.GetParametroByAlias(postModel);
            if (result.IsSuccess)
            {
                return Ok(result);
            }
            return BadRequest(result);
        }
    }
}
