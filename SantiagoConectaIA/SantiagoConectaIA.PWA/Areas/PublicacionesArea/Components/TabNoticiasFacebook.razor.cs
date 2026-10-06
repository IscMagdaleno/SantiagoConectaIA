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
        public bool PublicandoSiguiente { get; set; }
        public string EstadoSiguiente { get; set; } = "Publicando...";

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

        
        /// <summary>
        /// Publica la siguiente noticia con el mismo proceso que al seleccionarla a mano: se prepara el registro,
        /// se genera el post (imagen 1080x1360) y el texto con IA, y con la imagen nueva se publica en Facebook.
        /// </summary>
        public async Task PublicarSiguiente()
        {
            var confirmar = await DialogService.ShowMessageBox(
                "Publicar siguiente noticia",
                "Se tomará la noticia más reciente que todavía no se haya publicado, se generará su post (1080x1360) y el texto con IA, y se publicará en Facebook. ¿Continuar?",
                yesText: "Publicar",
                cancelText: "Cancelar");
            if (confirmar != true)
            {
                return;
            }

            PublicandoSiguiente = true;
            try
            {
                // 1. Cuál toca (el servidor valida que no esté publicada ya)
                EstadoSiguiente = "Buscando noticia...";
                var siguiente = await Publicaciones.GetSiguienteNoticia();
                if (!siguiente.IsSuccess || siguiente.Data == null || siguiente.Data.iIdNoticia <= 0)
                {
                    Snackbar.Add(string.IsNullOrWhiteSpace(siguiente.Message) ? "No hay una noticia pendiente de publicar." : siguiente.Message, Severity.Warning);
                    return;
                }

                // 2. Mismo proceso que al seleccionar: texto base, imagen y captura del post
                EstadoSiguiente = "Generando post...";
                StateHasChanged();
                UsarCapturaPost = true;
                await SeleccionarAsync(siguiente.Data);
                if (string.IsNullOrWhiteSpace(ImagenCapturadaUrl))
                {
                    Snackbar.Add("No se pudo generar la imagen del post, así que no se publicó.", Severity.Error);
                    return;
                }

                // 3. Texto con IA
                EstadoSiguiente = "Redactando con IA...";
                Mejorando = true;
                StateHasChanged();
                var texto = await Publicaciones.MejorarConIa(
                    siguiente.Data.iIdNoticia,
                    siguiente.Data.vchTitulo ?? string.Empty,
                    siguiente.Data.nvchContenido ?? string.Empty);
                Mejorando = false;
                if (!texto.IsSuccess || string.IsNullOrWhiteSpace(texto.Data))
                {
                    Snackbar.Add(string.IsNullOrWhiteSpace(texto.Message) ? "No se pudo redactar el texto con IA, así que no se publicó." : texto.Message, Severity.Error);
                    return;
                }

                Mensaje = texto.Data;

                // 4. Con la imagen nueva, a Facebook (el servidor valida y registra la publicación)
                EstadoSiguiente = "Publicando...";
                StateHasChanged();
                var result = await Publicaciones.PublicarNoticiaGenerada(siguiente.Data.iIdNoticia, Mensaje.Trim(), ImagenCapturadaUrl);

                // El API devuelve el estado actualizado (última noticia publicada) aun cuando no publica
                if (result.Data != null)
                {
                    Automatica = result.Data;
                }

                Snackbar.Add(
                    string.IsNullOrWhiteSpace(result.Message) ? "No se pudo publicar la noticia." : result.Message,
                    result.IsSuccess ? Severity.Success : Severity.Warning);
            }
            catch (Exception ex)
            {
                Snackbar.Add("No se pudo publicar la siguiente noticia: " + ex.Message, Severity.Error);
            }
            finally
            {
                Mejorando = false;
                PublicandoSiguiente = false;
                StateHasChanged();
            }
        }
        public async Task SeleccionarAsync(Noticia noticia)
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
