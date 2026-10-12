using Microsoft.AspNetCore.RateLimiting;

using System.Threading.RateLimiting;

namespace SantiagoConectaIA.API.Services
{
	/// <summary>
	/// Límite de peticiones por IP. Hay un límite general y tres políticas más estrictas para los
	/// endpoints anónimos que escriben datos, mandan mensajes o gastan cuota de IA.
	/// </summary>
	public static class PoliticasLimiteSolicitudes
	{
		/// <summary>Inicio de sesión, registro y envío de códigos por WhatsApp (10 por minuto).</summary>
		public const string Acceso = "acceso";

		/// <summary>Registro de emprendimientos, subida de logos y buzón ciudadano (30 por minuto).</summary>
		public const string EscrituraPublica = "escritura-publica";

		/// <summary>Chatbot público, que consume cuota de IA (15 por minuto).</summary>
		public const string Chat = "chat";

		private const int LimiteGeneralPorMinuto = 600;

		public static void Registrar(RateLimiterOptions options)
		{
			options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

			options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(contexto =>
				Ventana("general:" + Ip(contexto), LimiteGeneralPorMinuto));

			options.AddPolicy(Acceso, contexto => Ventana(Acceso + ":" + Ip(contexto), 10));
			options.AddPolicy(EscrituraPublica, contexto => Ventana(EscrituraPublica + ":" + Ip(contexto), 30));
			options.AddPolicy(Chat, contexto => Ventana(Chat + ":" + Ip(contexto), 15));

			options.OnRejected = async (contexto, cancellationToken) =>
			{
				if (contexto.Lease.TryGetMetadata(MetadataName.RetryAfter, out var espera))
				{
					contexto.HttpContext.Response.Headers.RetryAfter = ((int)Math.Ceiling(espera.TotalSeconds)).ToString();
				}

				await contexto.HttpContext.Response.WriteAsJsonAsync(
					new { isSuccess = false, message = "Demasiadas solicitudes. Intenta de nuevo en un momento." },
					cancellationToken);
			};
		}

		private static RateLimitPartition<string> Ventana(string clave, int limite) =>
			RateLimitPartition.GetFixedWindowLimiter(clave, _ => new FixedWindowRateLimiterOptions
			{
				PermitLimit = limite,
				Window = TimeSpan.FromMinutes(1),
				QueueLimit = 0
			});

		private static string Ip(HttpContext contexto) =>
			contexto.Connection.RemoteIpAddress?.ToString() ?? "desconocida";
	}
}
