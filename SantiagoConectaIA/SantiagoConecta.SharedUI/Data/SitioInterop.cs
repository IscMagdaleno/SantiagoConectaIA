using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace SantiagoConecta.SharedUI.Data
{
    /// <summary>
    /// Acceso al módulo wwwroot/js/sitio.js. Los errores de JS se ignoran: son mejoras visuales
    /// y no deben romper la página.
    /// </summary>
    public sealed class SitioInterop : IAsyncDisposable
    {
        private readonly Lazy<Task<IJSObjectReference>> _modulo;

        public SitioInterop(IJSRuntime js)
        {
            _modulo = new(() => js.InvokeAsync<IJSObjectReference>(
                "import", "./_content/SantiagoConecta.SharedUI/js/sitio.js").AsTask());
        }

        public Task QuitarSeoPorDefectoAsync() => InvocarAsync("quitarSeoPorDefecto");

        public Task IrASeccionAsync(string id, int desplazamiento = 90) => InvocarAsync("irASeccion", id, desplazamiento);

        public Task BloquearScrollAsync(bool bloquear) => InvocarAsync("bloquearScroll", bloquear);

        public Task IniciarParticulasAsync(ElementReference hero, ElementReference canvas, ElementReference glow)
            => InvocarAsync("iniciarParticulas", hero, canvas, glow);

        public Task DetenerParticulasAsync() => InvocarAsync("detenerParticulas");

        public Task IniciarReveladoAsync(ElementReference ancla) => InvocarAsync("iniciarRevelado", ancla);

        public Task DetenerReveladoAsync() => InvocarAsync("detenerRevelado");

        private async Task InvocarAsync(string funcion, params object?[] argumentos)
        {
            try
            {
                var modulo = await _modulo.Value;
                await modulo.InvokeVoidAsync(funcion, argumentos);
            }
            catch (JSException) { }
            catch (JSDisconnectedException) { }
            catch (InvalidOperationException) { }
        }

        public async ValueTask DisposeAsync()
        {
            if (_modulo.IsValueCreated)
            {
                try
                {
                    var modulo = await _modulo.Value;
                    await modulo.DisposeAsync();
                }
                catch (JSDisconnectedException) { }
            }
        }
    }
}
