using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using MudBlazor;
using SantiagoConectaIA.PWA.Areas.PublicacionesArea.Utiles;
using SantiagoConectaIA.Share.Objects.EventosModulo;
using SantiagoConectaIA.Share.PostModels.PublicacionesModule;
using SantiagoConectaIA.Share.Utilities;

namespace SantiagoConectaIA.PWA.Areas.PublicacionesArea.Components
{
    public partial class TabEventosFacebook : ComponentBase
    {
        [Inject] public MainPublicaciones Publicaciones { get; set; } = default!;
        [Inject] public ISnackbar Snackbar { get; set; } = default!;
        [Inject] public IDialogService DialogService { get; set; } = default!;
        [Inject] public IJSRuntime JSRuntime { get; set; } = default!;

        public List<Evento> LstEventos { get; set; } = new();
        public string FiltroTexto { get; set; } = string.Empty;
        public bool Cargando { get; set; } = true;
        public bool Mejorando { get; set; }
        public bool Publicando { get; set; }
        public bool GenerandoCaptura { get; set; }

        public bool UsarCapturaPost { get; set; } = true;
        public string? ImagenCapturadaUrl { get; set; }
        public string? ImagenPortadaBase64 { get; set; }

        public Evento? EventoSeleccionado { get; set; }
        public string Informacion { get; set; } = string.Empty;
        public string Mensaje { get; set; } = string.Empty;
        public string? ImagenOriginal { get; set; }
        public bool EditandoImagen { get; set; }

        public bool Ocupado => Mejorando || Publicando || EditandoImagen || GenerandoCaptura;
        public string? ImagenEfectiva => (UsarCapturaPost && !string.IsNullOrWhiteSpace(ImagenCapturadaUrl))
            ? ImagenCapturadaUrl
            : ImagenOriginal;
        public bool TieneImagen => !string.IsNullOrWhiteSpace(ImagenEfectiva);

        public bool PuedeMejorar => EventoSeleccionado != null && !string.IsNullOrWhiteSpace(Informacion) && !Ocupado;
        public bool PuedeEditarImagen => !string.IsNullOrWhiteSpace(ImagenOriginal) && !Ocupado;
        public bool PuedePublicar => EventoSeleccionado != null && TieneImagen && !string.IsNullOrWhiteSpace(Mensaje) && !Ocupado;

        public IEnumerable<Evento> EventosFiltrados =>
            (string.IsNullOrWhiteSpace(FiltroTexto)
                ? LstEventos
                : LstEventos.Where(e =>
                    (e.vchNombre?.Contains(FiltroTexto, StringComparison.OrdinalIgnoreCase) ?? false) ||
                    (e.vchLugar?.Contains(FiltroTexto, StringComparison.OrdinalIgnoreCase) ?? false)))
            .OrderByDescending(e => e.dtFechaInicio.ToUsuarioLocal());

        protected override async Task OnInitializedAsync()
        {
            var res = await Publicaciones.GetEventos();
            if (res.IsSuccess && res.Data != null)
            {
                LstEventos = res.Data;
            }
            else
            {
                Snackbar.Add(string.IsNullOrWhiteSpace(res.Message) ? "No se pudieron consultar los eventos." : res.Message, Severity.Error);
            }
            Cargando = false;
        }

        public async Task Seleccionar(Evento evento)
        {
            EventoSeleccionado = evento;
            ImagenOriginal = evento.vchImagenPortada;
            ImagenCapturadaUrl = null;
            Mensaje = string.Empty;
            Informacion = ArmarInformacionEvento(evento);

            if (UsarCapturaPost && EventoSeleccionado != null)
            {
                await GenerarCapturaPostAsync();
            }
        }

        private static string ArmarInformacionEvento(Evento e)
        {
            var fechaLocal = e.dtFechaInicio.ToUsuarioLocal();
            return $"Evento: {e.vchNombre}\r\n" +
                   $"Descripción: {e.nvchDescripcion}\r\n" +
                   $"Fecha: {fechaLocal:dd 'de' MMMM, yyyy}\r\n" +
                   $"Hora: {fechaLocal:hh:mm tt}\r\n" +
                   $"Lugar: {(!string.IsNullOrWhiteSpace(e.vchLugar) ? e.vchLugar : "Santiago Papasquiaro")}\r\n" +
                   $"Dirección: {e.vchDireccion}\r\n" +
                   $"Costo / Boletos: {(!string.IsNullOrWhiteSpace(e.vchCostoTexto) ? e.vchCostoTexto : "Entrada libre")}\r\n" +
                   $"Organizador: {e.vchOrganizador}";
        }

        public async Task OnUsarCapturaPostChanged(bool valor)
        {
            UsarCapturaPost = valor;
            if (UsarCapturaPost && string.IsNullOrWhiteSpace(ImagenCapturadaUrl) && EventoSeleccionado != null)
            {
                await GenerarCapturaPostAsync();
            }
        }

        public async Task GenerarCapturaPostAsync()
        {
            if (EventoSeleccionado == null) return;

            GenerandoCaptura = true;
            StateHasChanged();

            try
            {
                if (!string.IsNullOrWhiteSpace(EventoSeleccionado.vchImagenPortada) && EventoSeleccionado.vchImagenPortada.StartsWith("http", StringComparison.OrdinalIgnoreCase))
                {
                    var proxyResp = await Publicaciones.ProxyImageBase64(EventoSeleccionado.vchImagenPortada.Trim());
                    if (proxyResp.IsSuccess && !string.IsNullOrWhiteSpace(proxyResp.Data))
                    {
                        ImagenPortadaBase64 = proxyResp.Data;
                    }
                    else
                    {
                        ImagenPortadaBase64 = EventoSeleccionado.vchImagenPortada;
                    }
                }
                else
                {
                    ImagenPortadaBase64 = EventoSeleccionado.vchImagenPortada;
                }

                StateHasChanged();
                await Task.Delay(350);

                var base64 = await JSRuntime.InvokeAsync<string?>("postCapture.captureElementAsBase64", "post-card-evento-capture");
                if (!string.IsNullOrWhiteSpace(base64))
                {
                    var uploadResult = await Publicaciones.GuardarImagenBase64(base64, EventoSeleccionado.vchNombre ?? "evento");
                    if (uploadResult.IsSuccess && !string.IsNullOrWhiteSpace(uploadResult.Data))
                    {
                        ImagenCapturadaUrl = uploadResult.Data;
                        Snackbar.Add("Captura estilo Post (1080x1360) generada con éxito.", Severity.Success);
                    }
                    else
                    {
                        Snackbar.Add("Se capturó el evento pero hubo error al subirla: " + uploadResult.Message, Severity.Warning);
                    }
                }
            }
            catch (Exception ex)
            {
                Snackbar.Add("No se pudo generar la captura: " + ex.Message, Severity.Error);
            }
            finally
            {
                GenerandoCaptura = false;
                StateHasChanged();
            }
        }

        public async Task EditarImagen()
        {
            if (!PuedeEditarImagen) return;

            EditandoImagen = true;
            var editada = await EditorImagenIaDialog.AbrirAsync(DialogService, ImagenOriginal!);
            EditandoImagen = false;

            if (!string.IsNullOrWhiteSpace(editada))
            {
                ImagenOriginal = editada;
                if (UsarCapturaPost)
                {
                    await GenerarCapturaPostAsync();
                }
            }
        }

        public async Task MejorarConIa()
        {
            if (EventoSeleccionado == null || string.IsNullOrWhiteSpace(Informacion))
            {
                Snackbar.Add("Selecciona un evento para mejorar el texto.", Severity.Warning);
                return;
            }

            Mejorando = true;
            var model = new PostMejorarEvento
            {
                iIdEvento = EventoSeleccionado.iIdEvento,
                vchNombre = EventoSeleccionado.vchNombre,
                nvchDescripcion = EventoSeleccionado.nvchDescripcion,
                vchLugar = EventoSeleccionado.vchLugar,
                vchDireccion = EventoSeleccionado.vchDireccion,
                dtFechaInicio = $"{EventoSeleccionado.dtFechaInicio:dd 'de' MMMM, yyyy - hh:mm tt}",
                vchCostoTexto = EventoSeleccionado.vchCostoTexto,
                vchOrganizador = EventoSeleccionado.vchOrganizador
            };

            var result = await Publicaciones.MejorarEventoConIa(model);
            Mejorando = false;

            if (result.IsSuccess && !string.IsNullOrWhiteSpace(result.Data))
            {
                Mensaje = result.Data;
                Snackbar.Add("Texto redactado con éxito.", Severity.Success);
            }
            else
            {
                Snackbar.Add(string.IsNullOrWhiteSpace(result.Message) ? "No se pudo mejorar el texto." : result.Message, Severity.Error);
            }
        }

        public async Task Publicar()
        {
            if (!PuedePublicar) return;

            Publicando = true;
            var result = await Publicaciones.PublicarFacebook(Mensaje, ImagenEfectiva!);
            Publicando = false;

            if (result.IsSuccess)
            {
                Snackbar.Add("¡Evento publicado en Facebook con éxito!", Severity.Success);
            }
            else
            {
                Snackbar.Add("No se pudo publicar: " + result.Message, Severity.Error);
            }
        }
    }
}
