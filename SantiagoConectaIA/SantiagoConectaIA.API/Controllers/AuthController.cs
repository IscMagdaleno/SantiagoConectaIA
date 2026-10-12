using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

using SantiagoConectaIA.API.EngramaLevels.Domain.Interfaces.AuthModule;
using SantiagoConectaIA.API.Services;
using SantiagoConectaIA.Share.PostModels.AuthModulo;

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
		[EnableRateLimiting(PoliticasLimiteSolicitudes.Acceso)]
		public async Task<IActionResult> PostLoginUsuario([FromBody] PostLoginUsuario daoModel)
		{
			var result = await authDomain.Login(daoModel);
			return Ok(result);
		}

		[HttpPost("PostSaveUsuario")]
		[Authorize(Policy = AuthPolicies.Administracion)]
		public async Task<IActionResult> PostSaveUsuario([FromBody] PostSaveUsuario daoModel)
		{
			var result = await authDomain.SaveUsuario(daoModel);
			return Ok(result);
		}
	}
}
