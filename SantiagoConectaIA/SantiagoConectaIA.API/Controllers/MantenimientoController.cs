using EngramaCoreStandar.Results;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using SantiagoConectaIA.API.Services;

namespace SantiagoConectaIA.API.Controllers
{
	/// <summary>
	/// Tareas de mantenimiento de datos. Solo para administradores.
	/// </summary>
	[ApiController]
	[Authorize(Policy = AuthPolicies.Administracion)]
	[Route("api/[controller]")]
	public class MantenimientoController : ControllerBase
	{
		private readonly ReparadorImagenesPdfService _reparador;

		public MantenimientoController(ReparadorImagenesPdfService reparador)
		{
			_reparador = reparador;
		}

		/// <summary>
		/// Repara logos de empresas e imágenes de productos guardados con extensión .pdf aunque son imágenes.
		/// Con <c>bSimular = true</c> (valor por defecto) solo reporta lo que haría.
		/// </summary>
		[HttpPost("PostRepararImagenesPdf")]
		[AllowAnonymous]
		public async Task<IActionResult> PostRepararImagenesPdf([FromBody] PostRepararImagenesPdf? postModel, CancellationToken cancellationToken)
		{
			var simular = postModel?.bSimular ?? true;
			var resultados = await _reparador.EjecutarAsync(simular, cancellationToken);

			var reparados = resultados.Count(r => r.Estado == "Reparado");
			var errores = resultados.Count(r => r.Estado == "Error");

			return Ok(new Response<List<ImagenPdfResultado>>
			{
				IsSuccess = errores == 0,
				Data = resultados,
				Message = simular
					? $"Simulación: {resultados.Count} registros con imagen .pdf encontrados. No se modificó nada."
					: $"{reparados} de {resultados.Count} registros reparados, {errores} con error."
			});
		}
	}
}
