namespace SantiagoConectaIA.API.Services
{
	/// <summary>
	/// Cliente único de la API de Gemini. Resuelve la clave y el modelo desde los parámetros y manda la solicitud;
	/// cada servicio solo arma su prompt e interpreta la respuesta.
	/// </summary>
	public interface IGeminiClient
	{
		/// <summary>
		/// La clave siempre sale de <see cref="ParametrosAlias.Gemini"/> (valor 2). El modelo sale del valor 1 del
		/// parámetro <paramref name="aliasModelo"/>; si está vacío se usa <paramref name="modeloPorDefecto"/>.
		/// </summary>
		Task<GeminiConfiguracion> ObtenerConfiguracionAsync(string aliasModelo, string modeloPorDefecto, CancellationToken cancellationToken = default);

		/// <summary>
		/// Llama a <c>generateContent</c>. No atrapa excepciones (red, tiempo agotado): las maneja el servicio que llama.
		/// </summary>
		Task<GeminiRespuesta> GenerarContenidoAsync(GeminiConfiguracion configuracion, GeminiSolicitud solicitud, TimeSpan tiempoMaximo, CancellationToken cancellationToken = default);
	}
}
