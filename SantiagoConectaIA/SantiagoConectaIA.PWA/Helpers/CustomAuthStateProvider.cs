using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.JSInterop;

using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text.Json;

namespace SantiagoConectaIA.PWA.Helpers
{
	public class CustomAuthStateProvider : AuthenticationStateProvider
	{
		private readonly IJSRuntime _jsRuntime;
		private readonly HttpClient _httpClient;

		public CustomAuthStateProvider(IJSRuntime jsRuntime, HttpClient httpClient)
		{
			_jsRuntime = jsRuntime;
			_httpClient = httpClient;
		}

		public override async Task<AuthenticationState> GetAuthenticationStateAsync()
		{
			var token = await _jsRuntime.InvokeAsync<string>("localStorage.getItem", "authToken");

			var identity = new ClaimsIdentity();
			if (!string.IsNullOrEmpty(token))
			{
				try
				{
					var claims = ParseClaimsFromJwt(token).ToList();

					// La API ahora exige el JWT en los endpoints de administración: con un token vencido
					// todas las llamadas darían 401, así que se trata como sesión cerrada (redirige a Login).
					if (IsExpired(claims))
					{
						await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", "authToken");
						_httpClient.DefaultRequestHeaders.Authorization = null;
						return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
					}

					identity = new ClaimsIdentity(claims, "jwt");
					_httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
				}
				catch
				{
					await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", "authToken");
				}
			}

			var user = new ClaimsPrincipal(identity);
			return new AuthenticationState(user);
		}

		public void NotifyUserAuthentication(string token)
		{
			_httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
			var authenticatedUser = new ClaimsPrincipal(new ClaimsIdentity(ParseClaimsFromJwt(token), "jwt"));
			var authState = Task.FromResult(new AuthenticationState(authenticatedUser));
			NotifyAuthenticationStateChanged(authState);
		}

		public void NotifyUserLogout()
		{
			_httpClient.DefaultRequestHeaders.Authorization = null;
			var anonymousUser = new ClaimsPrincipal(new ClaimsIdentity());
			var authState = Task.FromResult(new AuthenticationState(anonymousUser));
			NotifyAuthenticationStateChanged(authState);
		}

		private static bool IsExpired(IEnumerable<Claim> claims)
		{
			var exp = claims.FirstOrDefault(c => c.Type == "exp")?.Value;
			return long.TryParse(exp, out var segundos)
				   && DateTimeOffset.FromUnixTimeSeconds(segundos) <= DateTimeOffset.UtcNow;
		}

		private IEnumerable<Claim> ParseClaimsFromJwt(string jwt)
		{
			var claims = new List<Claim>();
			var payload = jwt.Split('.')[1];
			var jsonBytes = ParseBase64WithoutPadding(payload);
			var keyValuePairs = JsonSerializer.Deserialize<Dictionary<string, object>>(jsonBytes);

			if (keyValuePairs != null)
			{
				claims.AddRange(keyValuePairs.Select(kvp => new Claim(kvp.Key, kvp.Value.ToString() ?? "")));
			}

			return claims;
		}

		private byte[] ParseBase64WithoutPadding(string base64)
		{
			switch (base64.Length % 4)
			{
				case 2: base64 += "=="; break;
				case 3: base64 += "="; break;
			}
			return Convert.FromBase64String(base64);
		}
	}
}
