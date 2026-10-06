using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using MudBlazor;
using SantiagoConectaIA.PWA.Areas.NoticiasArea.Utiles;
using SantiagoConectaIA.PWA.Areas.PublicacionesArea.Utiles;
using SantiagoConectaIA.Share.Objects.NoticiasModule;
using SantiagoConectaIA.Share.PostModels.PublicacionesModule;
using SantiagoConectaIA.Share.Utilities;

namespace SantiagoConectaIA.PWA.Areas.PublicacionesArea.Components
{
    public partial class TabNoticiasFacebook : ComponentBase
    {
        [Inject] public MainNoticias Noticias { get; set; } = default!;
        [Inject] public MainPublicaciones Publicaciones { get; set; } = default!;
        [Inject] public ISnackbar Snackbar { get; set; } = default!;
        [Inject] public IDialogService DialogService { get; set; } = default!;
        [Inject] public IJSRuntime JSRuntime { get; set; } = default!;

        public string FiltroTexto { get; set; } = string.Empty;
        public string Mensaje { get; set; } = string.Empty;
        public bool Cargando { get; set; } = true;
        public bool Mejorando { get; set; }
        public bool Publicando { get; set; }
        public bool EditandoImagen { get; set; }
        public bool GenerandoCaptura { get; set; }

        // Switch para activar captura formato Instagram/Facebook 1080x1360
        public bool UsarCapturaPost { get; set; } = true;
        public string? ImagenCapturadaUrl { get; set; }
        public string? ImagenPortadaBase64 { get; set; }

        public PublicacionAutomatica? Automatica { get; set; }
        public bool GuardandoAutomatica { get; set; }

        public Noticia? NoticiaSeleccionada { get; set; }
        public List<string> Imagenes { get; set; } = new();
        public string? ImagenSeleccionada { get; set; }

        public bool Ocupado => Mejorando || Publicando || EditandoImagen || GenerandoCaptura;
        public bool TieneImagen => !string.IsNullOrWhiteSpace(ImagenEfectiva);
        public string? ImagenEfectiva => (UsarCapturaPost && !string.IsNullOrWhiteSpace(ImagenCapturadaUrl)) 
            ? ImagenCapturadaUrl 
            : ImagenSeleccionada;

        public bool PuedePublicar => NoticiaSeleccionada != null && TieneImagen && !string.IsNullOrWhiteSpace(Mensaje) && !Ocupado;
        public bool PuedeMejorar => NoticiaSeleccionada != null && !Ocupado;
        public bool PuedeEditarImagen => !string.IsNullOrWhiteSpace(ImagenSeleccionada) && !Ocupado;

        public IEnumerable<Noticia> NoticiasFiltradas =>
            (string.IsNullOrWhiteSpace(FiltroTexto)
                ? Noticias.LstNoticias
                : Noticias.LstNoticias.Where(n =>
                    (n.vchTitulo?.Contains(FiltroTexto, StringComparison.OrdinalIgnoreCase) ?? false) ||
                    (n.nvchContenido?.Contains(FiltroTexto, StringComparison.OrdinalIgnoreCase) ?? false)))
            .OrderByDescending(n => n.dtFechaPublicacion);

        protected override async Task OnInitializedAsync()
        {
            var noticiasTask = Noticias.PostGetNoticias();
            var automaticaTask = Publicaciones.GetPublicacionAutomatica();
            await Task.WhenAll(noticiasTask, automaticaTask);

            var automatica = automaticaTask.Result;
            if (automatica.IsSuccess)
            {
                Automatica = automatica.Data;
            }

            Cargando = false;
        }

        public async Task CambiarAutomatica(bool activo)
        {
            GuardandoAutomatica = true;
            var result = await Publicaciones.SavePublicacionAutomatica(activo);
            GuardandoAutomatica = false;

            if (result.IsSuccess && result.Data != null)
            {
                Automatica = result.Data;
                Snackbar.Add(result.Message, Severity.Success);
            }
            else
            {
                Snackbar.Add(string.IsNullOrWhiteSpace(result.Message) ? "No se pudo cambiar la publicación automática." : result.Message, Severity.Error);
            }
        }

        public async Task Seleccionar(Noticia noticia)
        {
            NoticiaSeleccionada = noticia;
            Mensaje = ArmarMensaje(noticia);
            Imagenes = string.IsNullOrWhiteSpace(noticia.vchImagenPortada)
                ? new List<string>()
                : new List<string> { noticia.vchImagenPortada.Trim() };
            ImagenSeleccionada = Imagenes.FirstOrDefault();
            ImagenCapturadaUrl = null;

            if (UsarCapturaPost && !string.IsNullOrWhiteSpace(ImagenSeleccionada))
            {
                await GenerarCapturaPostAsync();
            }
        }

        public async Task OnUsarCapturaPostChanged(bool valor)
        {
            UsarCapturaPost = valor;
            if (UsarCapturaPost && string.IsNullOrWhiteSpace(ImagenCapturadaUrl) && NoticiaSeleccionada != null)
            {
                await GenerarCapturaPostAsync();
            }
        }

        public async Task GenerarCapturaPostAsync()
        {
            if (NoticiaSeleccionada == null) return;

            GenerandoCaptura = true;
            StateHasChanged();

            try
            {
                // Si la imagen seleccionada es una URL http/https remota, descargarla como Base64 mediante el proxy del backend para evitar error de CORS en html2canvas
                if (!string.IsNullOrWhiteSpace(ImagenSeleccionada) && ImagenSeleccionada.StartsWith("http", StringComparison.OrdinalIgnoreCase))
                {
                    var proxyResp = await Publicaciones.ProxyImageBase64(ImagenSeleccionada.Trim());
                    if (proxyResp.IsSuccess && !string.IsNullOrWhiteSpace(proxyResp.Data))
                    {
                        ImagenPortadaBase64 = proxyResp.Data;
                    }
                    else
                    {
                        ImagenPortadaBase64 = ImagenSeleccionada;
                    }
                }
                else
                {
                    ImagenPortadaBase64 = ImagenSeleccionada;
                }

                // Renderizar el DOM con la imagen en base64
                StateHasChanged();
                await Task.Delay(350);

                var base64 = await JSRuntime.InvokeAsync<string?>("postCapture.captureElementAsBase64", "post-card-noticia-capture");
                if (!string.IsNullOrWhiteSpace(base64))
                {
                    var uploadResult = await Publicaciones.GuardarImagenBase64(base64, NoticiaSeleccionada.vchTitulo ?? "noticia");
                    if (uploadResult.IsSuccess && !string.IsNullOrWhiteSpace(uploadResult.Data))
                    {
                        ImagenCapturadaUrl = uploadResult.Data;
                        Snackbar.Add("Captura estilo Post (1080x1360) generada con éxito.", Severity.Success);
                    }
                    else
                    {
                        Snackbar.Add("Se capturó el post pero hubo error al subirla: " + uploadResult.Message, Severity.Warning);
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

        public async void ElegirImagen(string url)
        {
            ImagenSeleccionada = url;
            if (UsarCapturaPost)
            {
                await GenerarCapturaPostAsync();
            }
        }

        public async Task EditarImagen()
        {
            if (!PuedeEditarImagen)
            {
                return;
            }

            EditandoImagen = true;
            var editada = await EditorImagenIaDialog.AbrirAsync(DialogService, ImagenSeleccionada!);
            EditandoImagen = false;

            if (!string.IsNullOrWhiteSpace(editada))
            {
                Imagenes.Add(editada);
                ImagenSeleccionada = editada;
                if (UsarCapturaPost)
                {
                    await GenerarCapturaPostAsync();
                }
            }
        }

        public async Task MejorarConIa()
        {
            if (NoticiaSeleccionada == null)
            {
                Snackbar.Add("Selecciona una noticia para mejorar el texto.", Severity.Warning);
                return;
            }

            Mejorando = true;
            var result = await Publicaciones.MejorarConIa(
                NoticiaSeleccionada.iIdNoticia,
                NoticiaSeleccionada.vchTitulo ?? string.Empty,
                NoticiaSeleccionada.nvchContenido ?? string.Empty);
            Mejorando = false;

            if (result.IsSuccess && !string.IsNullOrWhiteSpace(result.Data))
            {
                Mensaje = result.Data;
                Snackbar.Add("Texto listo. Revísalo y publícalo cuando quede bien.", Severity.Success);
            }
            else
            {
                Snackbar.Add(string.IsNullOrWhiteSpace(result.Message) ? "No se pudo mejorar el texto." : result.Message, Severity.Error);
            }
        }

        public async Task Publicar()
        {
            if (NoticiaSeleccionada == null || !TieneImagen || string.IsNullOrWhiteSpace(Mensaje))
            {
                Snackbar.Add("Selecciona una noticia con imagen y escribe el texto de la publicación.", Severity.Warning);
                return;
            }

            Publicando = true;
            var imagenParaPublicar = ImagenEfectiva!;
            var result = await Publicaciones.PublicarFacebook(Mensaje.Trim(), imagenParaPublicar.Trim());
            Publicando = false;

            if (result.IsSuccess)
            {
                Snackbar.Add("La noticia se envió a Facebook con éxito.", Severity.Success);
            }
            else
            {
                Snackbar.Add(string.IsNullOrWhiteSpace(result.Message) ? "No se pudo publicar la noticia." : result.Message, Severity.Error);
            }
        }

        private static string ArmarMensaje(Noticia noticia)
        {
            var titulo = (noticia.vchTitulo ?? string.Empty).Trim();
            var contenido = TextoPublicacion.ATextoPlano(noticia.nvchContenido);
            if (string.IsNullOrWhiteSpace(titulo))
            {
                return contenido;
            }

            if (string.IsNullOrWhiteSpace(contenido))
            {
                return titulo;
            }

            return $"{titulo}\n\n{contenido}";
        }
    }
}
