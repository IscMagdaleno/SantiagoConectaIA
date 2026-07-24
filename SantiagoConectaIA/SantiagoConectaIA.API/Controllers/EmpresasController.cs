using Microsoft.AspNetCore.Mvc;
using SantiagoConectaIA.API.EngramaLevels.Domain.Interfaces.EmpresasModule;
using SantiagoConectaIA.Share.PostClass.EmpresasModulo;
using SantiagoConectaIA.Share.PostModels.EmpresasModulo;
using SantiagoConectaIA.Share.Objects.EmpresasModulo;
using System.Threading.Tasks;

namespace SantiagoConectaIA.API.Controllers
{
    /// <summary>
    /// Controlador para el módulo de Empresas, siguiendo la metodología Engrama.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class EmpresasController : ControllerBase
    {
        private readonly IEmpresasDomain _empresasDomain;

        /// <summary>
        /// Inicializa el controlador con la dependencia de la capa de Dominio.
        /// </summary>
        public EmpresasController(IEmpresasDomain empresasDomain)
        {
            _empresasDomain = empresasDomain;
        }

        /// <summary>
        /// Consulta la lista de empresas.
        /// </summary>
        [HttpPost("PostGetEmpresas")]
        public async Task<IActionResult> PostGetEmpresas([FromBody] PostGetEmpresas postModel)
        {
            var result = await _empresasDomain.GetEmpresas(postModel);
            if (result.IsSuccess)
            {
                return Ok(result);
            }
            return BadRequest(result);
        }

        /// <summary>
        /// Guarda o actualiza un registro en la tabla Empresa.
        /// </summary>
        [HttpPost("PostSaveEmpresa")]
        public async Task<IActionResult> PostSaveEmpresa([FromBody] PostSaveEmpresa postModel)
        {
            var result = await _empresasDomain.SaveEmpresa(postModel);
            if (result.IsSuccess) return Ok(result);
            return BadRequest(result);
        }

        [HttpPost("PostGetCatalogoEmpresas")]
        public async Task<IActionResult> PostGetCatalogoEmpresas([FromBody] PostGetCatalogoEmpresa postModel)
        {
            var result = await _empresasDomain.GetCatalogoEmpresas(postModel);
            if (result.IsSuccess) return Ok(result);
            return BadRequest(result);
        }

        [HttpPost("PostGetEmpresaUbicaciones")]
        public async Task<IActionResult> PostGetEmpresaUbicaciones([FromBody] PostGetEmpresaUbicaciones postModel)
        {
            var result = await _empresasDomain.GetEmpresaUbicaciones(postModel);
            if (result.IsSuccess) return Ok(result);
            return BadRequest(result);
        }

        [HttpPost("PostSaveEmpresaUbicacion")]
        public async Task<IActionResult> PostSaveEmpresaUbicacion([FromBody] PostSaveEmpresaUbicacion postModel)
        {
            var result = await _empresasDomain.SaveEmpresaUbicacion(postModel);
            if (result.IsSuccess) return Ok(result);
            return BadRequest(result);
        }

        [HttpPost("PostGetEmpresaRedesSociales")]
        public async Task<IActionResult> PostGetEmpresaRedesSociales([FromBody] PostGetEmpresaRedesSociales postModel)
        {
            var result = await _empresasDomain.GetEmpresaRedesSociales(postModel);
            if (result.IsSuccess) return Ok(result);
            return BadRequest(result);
        }

        [HttpPost("PostSaveEmpresaRedSocial")]
        public async Task<IActionResult> PostSaveEmpresaRedSocial([FromBody] PostSaveEmpresaRedSocial postModel)
        {
            var result = await _empresasDomain.SaveEmpresaRedSocial(postModel);
            if (result.IsSuccess) return Ok(result);
            return BadRequest(result);
        }

        [HttpPost("PostGetCategoriasPorEmpresa")]
        public async Task<IActionResult> PostGetCategoriasPorEmpresa([FromBody] PostGetCategoriasPorEmpresa postModel)
        {
            var result = await _empresasDomain.GetCategoriasPorEmpresa(postModel);
            if (result.IsSuccess) return Ok(result);
            return BadRequest(result);
        }

        [HttpPost("PostSaveCategoriaCatalogo")]
        public async Task<IActionResult> PostSaveCategoriaCatalogo([FromBody] PostSaveCategoriaCatalogo postModel)
        {
            var result = await _empresasDomain.SaveCategoriaCatalogo(postModel);
            if (result.IsSuccess) return Ok(result);
            return BadRequest(result);
        }

        [HttpPost("PostGetProductosPorCategoria")]
        public async Task<IActionResult> PostGetProductosPorCategoria([FromBody] PostGetProductosPorCategoria postModel)
        {
            var result = await _empresasDomain.GetProductosPorCategoria(postModel);
            if (result.IsSuccess) return Ok(result);
            return BadRequest(result);
        }

        [HttpPost("PostSaveProductoServicio")]
        public async Task<IActionResult> PostSaveProductoServicio([FromBody] PostSaveProductoServicio postModel)
        {
            var result = await _empresasDomain.SaveProductoServicio(postModel);
            if (result.IsSuccess) return Ok(result);
            return BadRequest(result);
        }

        [HttpPost("PostGetConfiguracionVisual")]
        public async Task<IActionResult> PostGetConfiguracionVisual([FromBody] PostGetConfiguracionVisual postModel)
        {
            var result = await _empresasDomain.GetConfiguracionVisual(postModel);
            if (result.IsSuccess) return Ok(result);
            return BadRequest(result);
        }

        [HttpPost("PostSaveConfiguracionVisual")]
        public async Task<IActionResult> PostSaveConfiguracionVisual([FromBody] PostSaveConfiguracionVisual postModel)
        {
            var result = await _empresasDomain.SaveConfiguracionVisual(postModel);
            if (result.IsSuccess) return Ok(result);
            return BadRequest(result);
        }

        [HttpPost("PostSavePropietario")]
        public async Task<IActionResult> PostSavePropietario([FromBody] Propietario postModel)
        {
            var result = await _empresasDomain.SavePropietario(postModel);
            if (result.IsSuccess) return Ok(result);
            return BadRequest(result);
        }

        [HttpPost("PostGetPropietario")]
        public async Task<IActionResult> PostGetPropietario([FromBody] PostGetPropietario postModel)
        {
            var result = await _empresasDomain.GetPropietario(postModel);
            if (result.IsSuccess) return Ok(result);
            return BadRequest(result);
        }

        [HttpPost("PostSaveEmprendimientoFull")]
        public async Task<IActionResult> PostSaveEmprendimientoFull([FromBody] PostSaveEmprendimientoFull postModel)
        {
            var result = await _empresasDomain.SaveEmprendimientoFull(postModel);
            if (result.IsSuccess) return Ok(result);
            return BadRequest(result);
        }
    }
}
