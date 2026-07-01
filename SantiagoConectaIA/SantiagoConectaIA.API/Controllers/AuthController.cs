using Microsoft.AspNetCore.Mvc;
using SantiagoConectaIA.API.EngramaLevels.Domain.Interfaces.AuthModule;
using SantiagoConectaIA.Share.PostModels.AuthModulo;
using System.Threading.Tasks;

namespace SantiagoConectaIA.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthDomain authDomain;

        public AuthController(IAuthDomain authDomain)
        {
            this.authDomain = authDomain;
        }

        [HttpPost("PostLoginUsuario")]
        public async Task<IActionResult> PostLoginUsuario([FromBody] PostLoginUsuario daoModel)
        {
            var result = await authDomain.Login(daoModel);
            return Ok(result);
        }

        [HttpPost("PostSaveUsuario")]
        public async Task<IActionResult> PostSaveUsuario([FromBody] PostSaveUsuario daoModel)
        {
            var result = await authDomain.SaveUsuario(daoModel);
            return Ok(result);
        }
    }
}
