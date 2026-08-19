using Microsoft.AspNetCore.Mvc;
using SantiagoConectaIA.API.EngramaLevels.Domain.Interfaces;
using SantiagoConectaIA.Share.PostModels.CiudadanoModule;
using System.Threading.Tasks;

namespace SantiagoConectaIA.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CiudadanoController : ControllerBase
    {
        private readonly ICiudadanoDomain _ciudadanoDomain;

        public CiudadanoController(ICiudadanoDomain ciudadanoDomain)
        {
            _ciudadanoDomain = ciudadanoDomain;
        }

        /// <summary>
        /// Registra un ciudadano con alias, teléfono de 10 dígitos y PIN.
        /// </summary>
        [HttpPost("PostSaveCiudadano")]
        public async Task<IActionResult> PostSaveCiudadano([FromBody] PostSaveCiudadano postModel)
        {
            var result = await _ciudadanoDomain.Registrar(postModel ?? new PostSaveCiudadano());
            if (result.IsSuccess)
            {
                return Ok(result);
            }
            return BadRequest(result);
        }

        /// <summary>
        /// Envía un código de verificación por WhatsApp para completar el registro.
        /// </summary>
        [HttpPost("PostEnviarCodigoWhatsApp")]
        public async Task<IActionResult> PostEnviarCodigoWhatsApp([FromBody] PostSendCodigoCiudadano postModel)
        {
            var result = await _ciudadanoDomain.EnviarCodigoWhatsApp(postModel ?? new PostSendCodigoCiudadano());
            if (result.IsSuccess)
            {
                return Ok(result);
            }
            return BadRequest(result);
        }

        /// <summary>
        /// Inicia sesión de un ciudadano con teléfono y PIN.
        /// </summary>
        [HttpPost("PostLoginCiudadano")]
        public async Task<IActionResult> PostLoginCiudadano([FromBody] PostLoginCiudadano postModel)
        {
            var result = await _ciudadanoDomain.Login(postModel ?? new PostLoginCiudadano());
            if (result.IsSuccess)
            {
                return Ok(result);
            }
            return BadRequest(result);
        }
    }
}
