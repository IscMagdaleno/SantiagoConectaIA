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
        private const string EmailKey = "scia_ciudadano_email";
        private const string AvatarKey = "scia_ciudadano_avatar";
        private const string ProveedorKey = "scia_ciudadano_proveedor";
        private const string TelefonoKey = "scia_ciudadano_telefono";

        private readonly IJSRuntime _js;

        public CiudadanoSession(IJSRuntime js)
        {
            _js = js;
        }

        public int iIdCiudadano { get; private set; }
        public string Alias { get; private set; } = string.Empty;
        public string Email { get; private set; } = string.Empty;
        public string AvatarUrl { get; private set; } = string.Empty;
        public string Telefono { get; private set; } = string.Empty;
        public string ProveedorAuth { get; private set; } = "Local";
        public bool bCuentaVerificada { get; private set; } = true;
        public string Token { get; private set; } = string.Empty;
        public bool IsLoggedIn => iIdCiudadano > 0 && !string.IsNullOrWhiteSpace(Token);

        public event Action? Changed;

        public async Task LoadAsync()
        {
            try
            {
                Token = await _js.InvokeAsync<string>("localStorage.getItem", TokenKey) ?? string.Empty;
                Alias = await _js.InvokeAsync<string>("localStorage.getItem", AliasKey) ?? string.Empty;
                Email = await _js.InvokeAsync<string>("localStorage.getItem", EmailKey) ?? string.Empty;
                AvatarUrl = await _js.InvokeAsync<string>("localStorage.getItem", AvatarKey) ?? string.Empty;
                Telefono = await _js.InvokeAsync<string>("localStorage.getItem", TelefonoKey) ?? string.Empty;
                ProveedorAuth = await _js.InvokeAsync<string>("localStorage.getItem", ProveedorKey) ?? "Local";
                var idRaw = await _js.InvokeAsync<string>("localStorage.getItem", IdKey);
                iIdCiudadano = int.TryParse(idRaw, out var id) ? id : 0;
            }
            catch
            {
                Token = string.Empty;
                Alias = string.Empty;
                Email = string.Empty;
                AvatarUrl = string.Empty;
                Telefono = string.Empty;
                ProveedorAuth = "Local";
                iIdCiudadano = 0;
            }

            Changed?.Invoke();
        }

        public async Task SignInAsync(Ciudadano ciudadano)
        {
            iIdCiudadano = ciudadano.iIdCiudadano;
            Alias = ciudadano.vchAlias ?? string.Empty;
            Email = ciudadano.vchEmail ?? string.Empty;
            AvatarUrl = ciudadano.vchAvatarUrl ?? string.Empty;
            Telefono = ciudadano.vchTelefono ?? string.Empty;
            ProveedorAuth = ciudadano.vchProveedorAuth ?? "Local";
            bCuentaVerificada = ciudadano.bCuentaVerificada;
            Token = ciudadano.Token ?? string.Empty;

            await _js.InvokeVoidAsync("localStorage.setItem", TokenKey, Token);
            await _js.InvokeVoidAsync("localStorage.setItem", AliasKey, Alias);
            await _js.InvokeVoidAsync("localStorage.setItem", EmailKey, Email);
            await _js.InvokeVoidAsync("localStorage.setItem", AvatarKey, AvatarUrl);
            await _js.InvokeVoidAsync("localStorage.setItem", TelefonoKey, Telefono);
            await _js.InvokeVoidAsync("localStorage.setItem", ProveedorKey, ProveedorAuth);
            await _js.InvokeVoidAsync("localStorage.setItem", IdKey, iIdCiudadano.ToString());
            Changed?.Invoke();
        }

        public async Task SignOutAsync()
        {
            iIdCiudadano = 0;
            Alias = string.Empty;
            Email = string.Empty;
            AvatarUrl = string.Empty;
            Telefono = string.Empty;
            ProveedorAuth = "Local";
            Token = string.Empty;

            try
            {
                await _js.InvokeVoidAsync("localStorage.removeItem", TokenKey);
                await _js.InvokeVoidAsync("localStorage.removeItem", AliasKey);
                await _js.InvokeVoidAsync("localStorage.removeItem", EmailKey);
                await _js.InvokeVoidAsync("localStorage.removeItem", AvatarKey);
                await _js.InvokeVoidAsync("localStorage.removeItem", TelefonoKey);
                await _js.InvokeVoidAsync("localStorage.removeItem", ProveedorKey);
                await _js.InvokeVoidAsync("localStorage.removeItem", IdKey);
            }
            catch { }

            Changed?.Invoke();
        }
    }
}
