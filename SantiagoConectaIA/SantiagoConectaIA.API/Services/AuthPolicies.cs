using Microsoft.AspNetCore.Authorization;

namespace SantiagoConectaIA.API.Services
{
	/// <summary>
	/// Políticas de autorización de la API.
	/// </summary>
	public static class AuthPolicies
	{
		/// <summary>
		/// Usuarios del panel de administración (PWA). Los ciudadanos del sitio público también
		/// reciben un JWT válido (rol "Ciudadano"), por eso no basta con [Authorize]: se exige
		/// sesión iniciada y que el rol no sea Ciudadano.
		/// </summary>
		public const string Administracion = "Administracion";

		public static void Registrar(AuthorizationOptions options)
		{
			options.AddPolicy(Administracion, policy => policy
				.RequireAuthenticatedUser()
				.RequireAssertion(context => !context.User.IsInRole("Ciudadano")));
		}
	}
}
