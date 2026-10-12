using EngramaCoreStandar.Results;

using Microsoft.AspNetCore.Mvc;

namespace SantiagoConectaIA.API.Controllers
{
	/// <summary>
	/// Base de los controladores: unifica cómo se convierte un <see cref="Response{T}"/> en respuesta HTTP.
	/// </summary>
	public abstract class ApiControllerBase : ControllerBase
	{
		/// <summary>200 con el resultado si fue exitoso; si no, 400 con el mismo cuerpo.</summary>
		protected IActionResult Resultado<T>(Response<T> resultado) =>
			resultado.IsSuccess ? Ok(resultado) : BadRequest(resultado);
	}
}
