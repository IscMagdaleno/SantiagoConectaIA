using Microsoft.AspNetCore.Mvc;
using SantiagoConectaIA.DAL.Provider;
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
        private readonly ICatalogosProvider _catalogosProvider;

        public CatalogosController(ICatalogosProvider catalogosProvider)
        {
            _catalogosProvider = catalogosProvider;
        }

        /// <summary>
        /// Obtiene el listado de tipos de datos para metadatos.
        /// </summary>
        [HttpPost("PostGetTipoDatos")]
        public async Task<IActionResult> PostGetTipoDatos()
        {
            var data = await _catalogosProvider.GetTipoDatosAsync("tipos.datos.noticias");
            // Mapeo manual a DTO
            var dtos = data.Select(x => new TipoDatoDto
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
    }
}
