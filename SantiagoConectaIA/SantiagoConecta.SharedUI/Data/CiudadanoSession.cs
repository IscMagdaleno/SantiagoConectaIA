using Microsoft.JSInterop;
using SantiagoConectaIA.Share.Objects.CiudadanoModule;
using System;
using System.Threading.Tasks;

namespace SantiagoConecta.SharedUI.Data
{
    public class CiudadanoSession
    {
        private const string TokenKey = "scia_ciudadano_token";
        private const string AliasKey = "scia_ciudadano_alias";
        private const string IdKey = "scia_ciudadano_id";

        private readonly IJSRuntime _js;

        public CiudadanoSession(IJSRuntime js)
        {
            _js = js;
        }

        public int iIdCiudadano { get; private set; }
        public string Alias { get; private set; } = string.Empty;
        public string Token { get; private set; } = string.Empty;
        public bool IsLoggedIn => iIdCiudadano > 0 && !string.IsNullOrWhiteSpace(Token);

        public event Action? Changed;

        public async Task LoadAsync()
        {
            try
            {
                Token = await _js.InvokeAsync<string>("localStorage.getItem", TokenKey) ?? string.Empty;
                Alias = await _js.InvokeAsync<string>("localStorage.getItem", AliasKey) ?? string.Empty;
                var idRaw = await _js.InvokeAsync<string>("localStorage.getItem", IdKey);
                iIdCiudadano = int.TryParse(idRaw, out var id) ? id : 0;
            }
            catch
            {
                Token = string.Empty;
                Alias = string.Empty;
                iIdCiudadano = 0;
            }

            Changed?.Invoke();
        }

        public async Task SignInAsync(Ciudadano ciudadano)
        {
            iIdCiudadano = ciudadano.iIdCiudadano;
            Alias = ciudadano.vchAlias ?? string.Empty;
            Token = ciudadano.Token ?? string.Empty;

            await _js.InvokeVoidAsync("localStorage.setItem", TokenKey, Token);
            await _js.InvokeVoidAsync("localStorage.setItem", AliasKey, Alias);
            await _js.InvokeVoidAsync("localStorage.setItem", IdKey, iIdCiudadano.ToString());
            Changed?.Invoke();
        }

        public async Task SignOutAsync()
        {
            iIdCiudadano = 0;
            Alias = string.Empty;
            Token = string.Empty;

            try
            {
                await _js.InvokeVoidAsync("localStorage.removeItem", TokenKey);
                await _js.InvokeVoidAsync("localStorage.removeItem", AliasKey);
                await _js.InvokeVoidAsync("localStorage.removeItem", IdKey);
            }
            catch { }

            Changed?.Invoke();
        }
    }
}
