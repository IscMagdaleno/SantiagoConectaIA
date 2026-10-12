using SantiagoConectaIA.API.EngramaLevels.Domain.Interfaces;
using SantiagoConectaIA.Share.PostModels.LogsModulo;

using System.Text;

namespace SantiagoConectaIA.API.Middleware
{
	/// <summary>
	/// Bitácora de llamadas a la API. No guarda el cuerpo de los endpoints sensibles (contraseñas, tokens, códigos
	/// y secretos), ni archivos o contenido que no sea texto, y recorta lo demás para no llenar la base.
	/// </summary>
	public class ApiLoggingMiddleware
	{
		private const int LongitudMaximaCuerpo = 4000;
		private const string CuerpoOmitidoPorSeguridad = "[omitido por seguridad]";
		private const string CuerpoNoTexto = "[contenido que no es texto]";

		/// <summary>Rutas cuyo cuerpo (petición y respuesta) nunca se guarda.</summary>
		private static readonly string[] RutasSensibles =
		{
			"/api/Auth/",
			"/api/Ciudadano/",
			"/api/Catalogos/PostGetParametro",
			"/api/Publicaciones/",
			"/api/WhatsApp/webhook"
		};

		private readonly RequestDelegate _next;

		public ApiLoggingMiddleware(RequestDelegate next)
		{
			_next = next;
		}

		public async Task InvokeAsync(HttpContext context, ILogsDomain logsDomain)
		{
			var request = context.Request;

			if (HttpMethods.IsOptions(request.Method))
			{
				await _next(context);
				return;
			}

			var requestTime = DateTime.UtcNow;
			var esSensible = EsRutaSensible(request.Path);
			var requestBodyContent = await ReadRequestBody(request, esSensible);

			var originalBodyStream = context.Response.Body;
			using var responseBody = new MemoryStream();
			context.Response.Body = responseBody;

			try
			{
				await _next(context);
			}
			finally
			{
				var responseTime = DateTime.UtcNow;
				var duration = (int)(responseTime - requestTime).TotalMilliseconds;

				responseBody.Seek(0, SeekOrigin.Begin);
				var responseBodyContent = await ReadResponseBody(context.Response, responseBody, esSensible);
				responseBody.Seek(0, SeekOrigin.Begin);

				await responseBody.CopyToAsync(originalBodyStream);
				context.Response.Body = originalBodyStream;

				var log = new PostSaveApiCallLog
				{
					vchEndpoint = request.Path,
					vchRequestMethod = request.Method,
					dtRequestTimestamp = requestTime,
					nvchRequestBody = requestBodyContent,
					dtResponseTimestamp = responseTime,
					nvchResponseBody = responseBodyContent,
					bIsSuccess = context.Response.StatusCode >= 200 && context.Response.StatusCode < 300,
					iDurationMs = duration,
					vchHost = request.Host.ToString()
				};

				// La bitácora es obligatoria: se espera a que se guarde. Si falla, el dominio captura el error
				// para que la respuesta al cliente no se vea afectada.
				await logsDomain.SaveApiCallLog(log);
			}
		}

		internal static bool EsRutaSensible(PathString ruta) =>
			RutasSensibles.Any(prefijo => ruta.Value?.StartsWith(prefijo, StringComparison.OrdinalIgnoreCase) == true);

		private static bool EsTexto(string? contentType) =>
			!string.IsNullOrEmpty(contentType)
			&& (contentType.Contains("json", StringComparison.OrdinalIgnoreCase)
				|| contentType.StartsWith("text/", StringComparison.OrdinalIgnoreCase)
				|| contentType.Contains("xml", StringComparison.OrdinalIgnoreCase));

		private static string Recortar(string contenido) =>
			contenido.Length <= LongitudMaximaCuerpo
				? contenido
				: contenido[..LongitudMaximaCuerpo] + "... [recortado]";

		private static async Task<string> ReadRequestBody(HttpRequest request, bool esSensible)
		{
			if (esSensible)
			{
				return CuerpoOmitidoPorSeguridad;
			}

			// Sin cuerpo (GET) o con archivos (multipart): no se lee a memoria.
			if (request.ContentLength is null or 0 || !EsTexto(request.ContentType))
			{
				return request.ContentLength is null or 0 ? string.Empty : CuerpoNoTexto;
			}

			request.EnableBuffering();
			using var reader = new StreamReader(request.Body, Encoding.UTF8, true, 1024, true);
			var body = await reader.ReadToEndAsync();
			request.Body.Position = 0;
			return Recortar(body);
		}

		private static async Task<string> ReadResponseBody(HttpResponse response, MemoryStream body, bool esSensible)
		{
			if (esSensible)
			{
				return CuerpoOmitidoPorSeguridad;
			}

			if (body.Length == 0)
			{
				return string.Empty;
			}

			if (!EsTexto(response.ContentType))
			{
				return CuerpoNoTexto;
			}

			var contenido = await new StreamReader(body, Encoding.UTF8, true, 1024, true).ReadToEndAsync();
			return Recortar(contenido);
		}
	}
}
