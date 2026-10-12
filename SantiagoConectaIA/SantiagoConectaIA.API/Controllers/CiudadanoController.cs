using Microsoft.AspNetCore.Mvc;
using SantiagoConectaIA.API.Services;
using Microsoft.AspNetCore.RateLimiting;

using SantiagoConectaIA.API.EngramaLevels.Domain.Interfaces;
using SantiagoConectaIA.Share.PostModels.CiudadanoModule;

namespace SantiagoConectaIA.API.Controllers
{
	[ApiController]
	[Route("api/[controller]")]
	public class CiudadanoController : ApiControllerBase
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
		[EnableRateLimiting(PoliticasLimiteSolicitudes.Acceso)]
		public async Task<IActionResult> PostSaveCiudadano([FromBody] PostSaveCiudadano postModel)
		{
			var result = await _ciudadanoDomain.Registrar(postModel ?? new PostSaveCiudadano());
			return Resultado(result);
		}

		/// <summary>
		/// Envía un código de verificación por WhatsApp para completar el registro.
		/// </summary>
		[HttpPost("PostEnviarCodigoWhatsApp")]
		[EnableRateLimiting(PoliticasLimiteSolicitudes.Acceso)]
		public async Task<IActionResult> PostEnviarCodigoWhatsApp([FromBody] PostSendCodigoCiudadano postModel)
		{
			var result = await _ciudadanoDomain.EnviarCodigoWhatsApp(postModel ?? new PostSendCodigoCiudadano());
			return Resultado(result);
		}

		/// <summary>
		/// Inicia sesión de un ciudadano con teléfono y PIN.
		/// </summary>
		[HttpPost("PostLoginCiudadano")]
		[EnableRateLimiting(PoliticasLimiteSolicitudes.Acceso)]
		public async Task<IActionResult> PostLoginCiudadano([FromBody] PostLoginCiudadano postModel)
		{
			var result = await _ciudadanoDomain.Login(postModel ?? new PostLoginCiudadano());
			return Resultado(result);
		}

		/// <summary>
		/// Inicia sesión o registra un ciudadano mediante Google o Facebook.
		/// </summary>
		[HttpPost("PostExternalLogin")]
		[EnableRateLimiting(PoliticasLimiteSolicitudes.Acceso)]
		public async Task<IActionResult> PostExternalLogin([FromBody] PostExternalLoginCiudadano postModel)
		{
			var result = await _ciudadanoDomain.ExternalLogin(postModel ?? new PostExternalLoginCiudadano());
			return Resultado(result);
		}

		/// <summary>
		/// Obtiene el ClientId o AppId de un proveedor social (Google o Facebook).
		/// </summary>
		[HttpGet("GetSocialAuthId")]
		public async Task<IActionResult> GetSocialAuthId([FromQuery] string proveedor)
		{
			var result = await _ciudadanoDomain.GetSocialAuthId(proveedor ?? string.Empty);
			return Resultado(result);
		}
	}
}
