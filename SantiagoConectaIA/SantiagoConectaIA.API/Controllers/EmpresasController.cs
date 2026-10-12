using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

using SantiagoConectaIA.API.EngramaLevels.Domain.Interfaces.EmpresasModule;
using SantiagoConectaIA.API.Services;
using SantiagoConectaIA.Share.PostClass.EmpresasModulo;
using SantiagoConectaIA.Share.PostModels.EmpresasModulo;

namespace SantiagoConectaIA.API.Controllers
{
	/// <summary>
	/// Controlador para el módulo de Empresas, siguiendo la metodología Engrama.
	/// </summary>
	[ApiController]
	[Route("api/[controller]")]
	public class EmpresasController : ApiControllerBase
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
			return Resultado(result);
		}

		/// <summary>
		/// Guarda o actualiza un registro en la tabla Empresa.
		/// </summary>
		[HttpPost("PostSaveEmpresa")]
		[Authorize(Policy = AuthPolicies.Administracion)]
		public async Task<IActionResult> PostSaveEmpresa([FromBody] PostSaveEmpresa postModel)
		{
			var result = await _empresasDomain.SaveEmpresa(postModel);
			return Resultado(result);
		}

		[HttpPost("PostGetCatalogoEmpresas")]
		public async Task<IActionResult> PostGetCatalogoEmpresas([FromBody] PostGetCatalogoEmpresa postModel)
		{
			var result = await _empresasDomain.GetCatalogoEmpresas(postModel);
			return Resultado(result);
		}

		[HttpPost("PostGetEmpresaUbicaciones")]
		public async Task<IActionResult> PostGetEmpresaUbicaciones([FromBody] PostGetEmpresaUbicaciones postModel)
		{
			var result = await _empresasDomain.GetEmpresaUbicaciones(postModel);
			return Resultado(result);
		}

		[HttpPost("PostSaveEmpresaUbicacion")]
		[Authorize(Policy = AuthPolicies.Administracion)]
		public async Task<IActionResult> PostSaveEmpresaUbicacion([FromBody] PostSaveEmpresaUbicacion postModel)
		{
			var result = await _empresasDomain.SaveEmpresaUbicacion(postModel);
			return Resultado(result);
		}

		[HttpPost("PostGetEmpresaRedesSociales")]
		public async Task<IActionResult> PostGetEmpresaRedesSociales([FromBody] PostGetEmpresaRedesSociales postModel)
		{
			var result = await _empresasDomain.GetEmpresaRedesSociales(postModel);
			return Resultado(result);
		}

		[HttpPost("PostSaveEmpresaRedSocial")]
		[Authorize(Policy = AuthPolicies.Administracion)]
		public async Task<IActionResult> PostSaveEmpresaRedSocial([FromBody] PostSaveEmpresaRedSocial postModel)
		{
			var result = await _empresasDomain.SaveEmpresaRedSocial(postModel);
			return Resultado(result);
		}

		[HttpPost("PostGetCategoriasPorEmpresa")]
		public async Task<IActionResult> PostGetCategoriasPorEmpresa([FromBody] PostGetCategoriasPorEmpresa postModel)
		{
			var result = await _empresasDomain.GetCategoriasPorEmpresa(postModel);
			return Resultado(result);
		}

		[HttpPost("PostSaveCategoriaCatalogo")]
		[Authorize(Policy = AuthPolicies.Administracion)]
		public async Task<IActionResult> PostSaveCategoriaCatalogo([FromBody] PostSaveCategoriaCatalogo postModel)
		{
			var result = await _empresasDomain.SaveCategoriaCatalogo(postModel);
			return Resultado(result);
		}

		[HttpPost("PostGetProductosPorCategoria")]
		public async Task<IActionResult> PostGetProductosPorCategoria([FromBody] PostGetProductosPorCategoria postModel)
		{
			var result = await _empresasDomain.GetProductosPorCategoria(postModel);
			return Resultado(result);
		}

		[HttpPost("PostSaveProductoServicio")]
		[Authorize(Policy = AuthPolicies.Administracion)]
		public async Task<IActionResult> PostSaveProductoServicio([FromBody] PostSaveProductoServicio postModel)
		{
			var result = await _empresasDomain.SaveProductoServicio(postModel);
			return Resultado(result);
		}

		[HttpPost("PostGetConfiguracionVisual")]
		public async Task<IActionResult> PostGetConfiguracionVisual([FromBody] PostGetConfiguracionVisual postModel)
		{
			var result = await _empresasDomain.GetConfiguracionVisual(postModel);
			return Resultado(result);
		}

		[HttpPost("PostSaveConfiguracionVisual")]
		[Authorize(Policy = AuthPolicies.Administracion)]
		public async Task<IActionResult> PostSaveConfiguracionVisual([FromBody] PostSaveConfiguracionVisual postModel)
		{
			var result = await _empresasDomain.SaveConfiguracionVisual(postModel);
			return Resultado(result);
		}

		[HttpPost("PostGetPropietario")]
		[Authorize(Policy = AuthPolicies.Administracion)]
		public async Task<IActionResult> PostGetPropietario([FromBody] PostGetPropietario postModel)
		{
			var result = await _empresasDomain.GetPropietario(postModel);
			return Resultado(result);
		}

		[HttpPost("PostSaveEmprendimientoFull")]
		[EnableRateLimiting(PoliticasLimiteSolicitudes.EscrituraPublica)]
		public async Task<IActionResult> PostSaveEmprendimientoFull([FromBody] PostSaveEmprendimientoFull postModel)
		{
			var result = await _empresasDomain.SaveEmprendimientoFull(postModel);
			return Resultado(result);
		}

		[HttpPost("PostGetEmpresasPropietarioByCorreo")]
		public async Task<IActionResult> PostGetEmpresasPropietarioByCorreo([FromBody] PostGetEmpresasPropietarioByCorreo postModel)
		{
			var result = await _empresasDomain.GetEmpresasPropietarioByCorreo(postModel);
			return Resultado(result);
		}

		[HttpPost("PostGetEmprendimientoFullById")]
		public async Task<IActionResult> PostGetEmprendimientoFullById([FromBody] PostGetEmprendimientoFullById postModel)
		{
			var result = await _empresasDomain.GetEmprendimientoFullById(postModel);
			return Resultado(result);
		}
	}
}
