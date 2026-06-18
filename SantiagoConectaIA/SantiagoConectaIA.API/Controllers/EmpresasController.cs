using Microsoft.AspNetCore.Mvc;
using SantiagoConectaIA.API.EngramaLevels.Domain.Interfaces.EmpresasModule;
using SantiagoConectaIA.Share.PostClass.EmpresasModulo;
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
            if (result.IsSuccess)
            {
                return Ok(result);
            }
            return BadRequest(result);
        }
    }
}
