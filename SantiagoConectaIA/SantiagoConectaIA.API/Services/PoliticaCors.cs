using Microsoft.AspNetCore.Cors.Infrastructure;

namespace SantiagoConectaIA.API.Services
{
	/// <summary>
	/// Define qué orígenes del navegador pueden llamar a la API (sitio público y PWA de administración).
	/// Si el sitio o el PWA se publican en otro dominio, se agrega en la configuración
	/// <c>Cors:OrigenesPermitidos</c> (en Azure: <c>Cors__OrigenesPermitidos__0</c>) sin tocar el código.
	/// </summary>
	public static class PoliticaCors
	{
		public const string Nombre = "SitiosSantiagoConecta";

		private const string DominioSitio = "santiagopapasquiaro.com.mx";
		private const string SufijoStaticWebApps = ".azurestaticapps.net";

		/// <summary>Prefijos de las Azure Static Web Apps del sitio público y del PWA (incluye sus ambientes de preview).</summary>
		private static readonly string[] PrefijosStaticWebApps =
		{
			"kind-flower-07d90ab0f",
			"nice-island-05284310f",
			"thankful-water-06e49b90f"
		};

		public static void Registrar(CorsOptions options, IConfiguration configuracion, IHostEnvironment entorno)
		{
			var configurados = (configuracion.GetSection("Cors:OrigenesPermitidos").Get<string[]>() ?? Array.Empty<string>())
				.Select(NormalizarOrigen)
				.ToHashSet(StringComparer.OrdinalIgnoreCase);
			var desarrollo = entorno.IsDevelopment();

			options.AddPolicy(Nombre, policy => policy
				.AllowAnyMethod()
				.AllowAnyHeader()
				.SetIsOriginAllowed(origen => EsOrigenPermitido(origen, configurados, desarrollo))
				.AllowCredentials());
		}

		internal static bool EsOrigenPermitido(string origen, ISet<string> configurados, bool desarrollo)
		{
			if (!Uri.TryCreate(origen, UriKind.Absolute, out var uri))
			{
				return false;
			}

			if (configurados.Contains(NormalizarOrigen(origen)))
			{
				return true;
			}

			if (desarrollo && uri.IsLoopback)
			{
				return true;
			}

			if (uri.Scheme != Uri.UriSchemeHttps)
			{
				return false;
			}

			var host = uri.Host;
			if (host.Equals(DominioSitio, StringComparison.OrdinalIgnoreCase)
				|| host.EndsWith("." + DominioSitio, StringComparison.OrdinalIgnoreCase))
			{
				return true;
			}

			return host.EndsWith(SufijoStaticWebApps, StringComparison.OrdinalIgnoreCase)
				&& PrefijosStaticWebApps.Any(prefijo => host.StartsWith(prefijo, StringComparison.OrdinalIgnoreCase));
		}

		private static string NormalizarOrigen(string origen) => origen.Trim().TrimEnd('/');
	}
}
