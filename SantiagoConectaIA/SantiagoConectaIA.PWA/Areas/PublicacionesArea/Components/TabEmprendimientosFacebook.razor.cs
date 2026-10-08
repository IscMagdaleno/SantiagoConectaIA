using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using MudBlazor;
using SantiagoConectaIA.PWA.Areas.PublicacionesArea.Utiles;
using SantiagoConectaIA.Share.Objects.EmpresasModulo;
using SantiagoConectaIA.Share.PostModels.EmpresasModulo;
using SantiagoConectaIA.Share.PostModels.PublicacionesModule;
using SantiagoConectaIA.Share.Utilities;

namespace SantiagoConectaIA.PWA.Areas.PublicacionesArea.Components
{
    public partial class TabEmprendimientosFacebook : ComponentBase
    {
        [Inject] public MainPublicaciones Publicaciones { get; set; } = default!;
        [Inject] public ISnackbar Snackbar { get; set; } = default!;
        [Inject] public IDialogService DialogService { get; set; } = default!;
        [Inject] public IJSRuntime JSRuntime { get; set; } = default!;

        public List<Empresa> LstEmprendimientos { get; set; } = new();
        public string FiltroTexto { get; set; } = string.Empty;
        public bool Cargando { get; set; } = true;
        public bool CargandoDetalle { get; set; }
        public bool Mejorando { get; set; }
        public bool Publicando { get; set; }
        public bool GenerandoCaptura { get; set; }

        public bool UsarCapturaPost { get; set; } = true;
        public string? ImagenCapturadaUrl { get; set; }
        public string? LogoBase64 { get; set; }

        public PublicacionAutomaticaEmprendimientos? Automatica { get; set; }
        public bool GuardandoAutomatica { get; set; }
        public bool PublicandoSiguiente { get; set; }
        public string EstadoSiguiente { get; set; } = "Publicando...";

        public Empresa? EmprendimientoSeleccionado { get; set; }
        public PostSaveEmprendimientoFull? Detalle { get; set; }
        public string Informacion { get; set; } = string.Empty;
        public string Mensaje { get; set; } = string.Empty;
        public List<string> Imagenes { get; set; } = new();
        public string? ImagenSeleccionada { get; set; }
        public bool EditandoImagen { get; set; }

        public bool Ocupado => Mejorando || Publicando || EditandoImagen || GenerandoCaptura;
        public string? ImagenEfectiva => (UsarCapturaPost && !string.IsNullOrWhiteSpace(ImagenCapturadaUrl))
            ? ImagenCapturadaUrl
            : ImagenSeleccionada;
        public bool TieneImagen => !string.IsNullOrWhiteSpace(ImagenEfectiva);

        public bool PuedeMejorar => EmprendimientoSeleccionado != null && !CargandoDetalle && !string.IsNullOrWhiteSpace(Informacion) && !Ocupado;
        public bool PuedeEditarImagen => !string.IsNullOrWhiteSpace(ImagenSeleccionada) && !Ocupado;
        public bool PuedePublicar => EmprendimientoSeleccionado != null && TieneImagen && !string.IsNullOrWhiteSpace(Mensaje) && !Ocupado;

        public IEnumerable<Empresa> EmprendimientosFiltrados =>
            (string.IsNullOrWhiteSpace(FiltroTexto)
                ? LstEmprendimientos
                : LstEmprendimientos.Where(e =>
                    (e.vchNombreComercial?.Contains(FiltroTexto, StringComparison.OrdinalIgnoreCase) ?? false) ||
                    (e.vchSlogan?.Contains(FiltroTexto, StringComparison.OrdinalIgnoreCase) ?? false)))
            .OrderBy(e => e.iIdEmpresa);

        protected override async Task OnInitializedAsync()
        {
            var emprendimientosTask = Publicaciones.GetEmprendimientos();
            var automaticaTask = Publicaciones.GetPublicacionAutomaticaEmprendimientos();
            await Task.WhenAll(emprendimientosTask, automaticaTask);

            var result = emprendimientosTask.Result;
            if (result.IsSuccess && result.Data != null)
            {
                LstEmprendimientos = result.Data;
            }
            else
            {
                Snackbar.Add(string.IsNullOrWhiteSpace(result.Message) ? "No se pudieron cargar los emprendimientos." : result.Message, Severity.Error);
            }

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
            var result = await Publicaciones.SavePublicacionAutomaticaEmprendimientos(activo);
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
        /// Publica el siguiente emprendimiento de la rotación con el mismo proceso que al seleccionarlo a mano:
        /// se carga su detalle, se genera el post (imagen 1080x1360) y el texto con IA, y con la imagen nueva
        /// se publica en Facebook.
        /// </summary>
        public async Task PublicarSiguiente()
        {
            var siguiente = Automatica != null && !string.IsNullOrWhiteSpace(Automatica.vchSiguienteEmpresa)
                ? $"#{Automatica.iIdSiguienteEmpresa} {Automatica.vchSiguienteEmpresa}"
                : "el siguiente emprendimiento";

            var confirmar = await DialogService.ShowMessageBox(
                "Publicar siguiente emprendimiento",
                $"Se generará el post (1080x1360) y el texto con IA de {siguiente}, y se publicará en Facebook. Después la rotación avanza al que sigue. ¿Continuar?",
                yesText: "Publicar",
                cancelText: "Cancelar");
            if (confirmar != true)
            {
                return;
            }

            PublicandoSiguiente = true;
            try
            {
                // 1. Cuál toca según el ID donde se quedó la rotación (el servidor valida)
                EstadoSiguiente = "Buscando emprendimiento...";
                var toca = await Publicaciones.GetSiguienteEmprendimiento();
                if (!toca.IsSuccess || toca.Data == null || toca.Data.iIdEmpresa <= 0)
                {
                    Snackbar.Add(string.IsNullOrWhiteSpace(toca.Message) ? "No hay un emprendimiento pendiente de publicar." : toca.Message, Severity.Warning);
                    return;
                }

                // 2. Mismo proceso que al seleccionar: detalle, información, imágenes y captura del post
                EstadoSiguiente = "Generando post...";
                StateHasChanged();
                var empresa = LstEmprendimientos.FirstOrDefault(e => e.iIdEmpresa == toca.Data.iIdEmpresa) ?? toca.Data;
                UsarCapturaPost = true;
                await Seleccionar(empresa);
                if (Detalle == null || EmprendimientoSeleccionado?.iIdEmpresa != empresa.iIdEmpresa)
                {
                    Snackbar.Add("No se pudo cargar la información del emprendimiento, así que no se publicó.", Severity.Error);
                    return;
                }

                if (string.IsNullOrWhiteSpace(ImagenCapturadaUrl))
                {
                    Snackbar.Add("No se pudo generar la imagen del post, así que no se publicó.", Severity.Error);
                    return;
                }

                // 3. Texto con IA
                EstadoSiguiente = "Redactando con IA...";
                Mejorando = true;
                StateHasChanged();
                var texto = await Publicaciones.MejorarEmprendimientoConIa(
                    empresa.iIdEmpresa,
                    empresa.vchNombreComercial ?? string.Empty,
                    Informacion);
                Mejorando = false;
                if (!texto.IsSuccess || string.IsNullOrWhiteSpace(texto.Data))
                {
                    Snackbar.Add(string.IsNullOrWhiteSpace(texto.Message) ? "No se pudo redactar el texto con IA, así que no se publicó." : texto.Message, Severity.Error);
                    return;
                }

                Mensaje = texto.Data;

                // 4. Con la imagen nueva, a Facebook (el servidor valida y avanza la rotación)
                EstadoSiguiente = "Publicando...";
                StateHasChanged();
                var result = await Publicaciones.PublicarEmprendimientoGenerado(empresa.iIdEmpresa, Mensaje.Trim(), ImagenCapturadaUrl);

                // El API devuelve el estado actualizado (cuál sigue) aun cuando no publica
                if (result.Data != null)
                {
                    Automatica = result.Data;
                }

                Snackbar.Add(
                    string.IsNullOrWhiteSpace(result.Message) ? "No se pudo publicar el emprendimiento." : result.Message,
                    result.IsSuccess ? Severity.Success : Severity.Warning);
            }
            catch (Exception ex)
            {
                Snackbar.Add("No se pudo publicar el siguiente emprendimiento: " + ex.Message, Severity.Error);
            }
            finally
            {
                Mejorando = false;
                PublicandoSiguiente = false;
                StateHasChanged();
            }
        }
        public async Task Seleccionar(Empresa empresa)
        {
            EmprendimientoSeleccionado = empresa;
            Detalle = null;
            Informacion = string.Empty;
            Mensaje = string.Empty;
            Imagenes = new List<string>();
            ImagenSeleccionada = null;
            CargandoDetalle = true;

            var result = await Publicaciones.GetEmprendimientoCompleto(empresa);
            if (EmprendimientoSeleccionado?.iIdEmpresa != empresa.iIdEmpresa)
            {
                return;
            }

            CargandoDetalle = false;
            if (!result.IsSuccess || result.Data == null)
            {
                Snackbar.Add(string.IsNullOrWhiteSpace(result.Message) ? "No se pudo obtener la información del emprendimiento." : result.Message, Severity.Error);
                return;
            }

            var detalle = result.Data;
            if (detalle.Empresa == null || detalle.Empresa.iIdEmpresa <= 0)
            {
                detalle.Empresa = empresa;
            }

            Detalle = detalle;
            Informacion = EmprendimientoTextoBuilder.ArmarInformacion(detalle);
            Imagenes = EmprendimientoTextoBuilder.ArmarImagenes(detalle);
            ImagenSeleccionada = Imagenes.FirstOrDefault();
            ImagenCapturadaUrl = null;

            if (UsarCapturaPost && Detalle != null)
            {
                await GenerarCapturaPostAsync();
            }
        }

        public Dictionary<string, string> ProductosImagenesBase64 { get; set; } = new();

        public async Task OnUsarCapturaPostChanged(bool valor)
        {
            UsarCapturaPost = valor;
            if (UsarCapturaPost && string.IsNullOrWhiteSpace(ImagenCapturadaUrl) && Detalle != null)
            {
                await GenerarCapturaPostAsync();
            }
        }

        public async Task GenerarCapturaPostAsync()
        {
            if (Detalle == null) return;

            GenerandoCaptura = true;
            StateHasChanged();

            try
            {
                // 1. Recolectar URLs que necesitan proxy (logo + fotos de productos)
                var urlsParaProxy = new List<string>();

                if (!string.IsNullOrWhiteSpace(Detalle.Empresa?.vchLogoUrl) && Detalle.Empresa.vchLogoUrl.StartsWith("http", StringComparison.OrdinalIgnoreCase))
                {
                    urlsParaProxy.Add(Detalle.Empresa.vchLogoUrl.Trim());
                }

                if (Detalle.Categorias != null)
                {
                    var fotosProds = Detalle.Categorias
                        .SelectMany(c => c.Productos ?? new List<ProductoServicio>())
                        .Where(p => p.bEstatus && !string.IsNullOrWhiteSpace(p.vchImagenUrl) && p.vchImagenUrl.StartsWith("http", StringComparison.OrdinalIgnoreCase))
                        .Select(p => p.vchImagenUrl!.Trim())
                        .Distinct();

                    urlsParaProxy.AddRange(fotosProds);
                }

                if (urlsParaProxy.Any())
                {
                    var batchResp = await Publicaciones.ProxyBatchImagesBase64(urlsParaProxy.Distinct().ToList());
                    if (batchResp.IsSuccess && batchResp.Data != null)
                    {
                        ProductosImagenesBase64 = batchResp.Data;

                        if (!string.IsNullOrWhiteSpace(Detalle.Empresa?.vchLogoUrl) && ProductosImagenesBase64.TryGetValue(Detalle.Empresa.vchLogoUrl.Trim(), out var logoBase64))
                        {
                            LogoBase64 = logoBase64;
                        }
                        else
                        {
                            LogoBase64 = Detalle.Empresa?.vchLogoUrl;
                        }
                    }
                    else
                    {
                        LogoBase64 = Detalle.Empresa?.vchLogoUrl;
                    }
                }
                else
                {
                    LogoBase64 = Detalle.Empresa?.vchLogoUrl;
                }

                StateHasChanged();
                await Task.Delay(400); // Dar tiempo para que el DOM pinte todas las imágenes en Base64

                var base64 = await JSRuntime.InvokeAsync<string?>("postCapture.captureElementAsBase64", "post-card-emprendimiento-capture");
                if (!string.IsNullOrWhiteSpace(base64))
                {
                    var uploadResult = await Publicaciones.GuardarImagenBase64(base64, Detalle.Empresa?.vchNombreComercial ?? "emprendimiento");
                    if (uploadResult.IsSuccess && !string.IsNullOrWhiteSpace(uploadResult.Data))
                    {
                        ImagenCapturadaUrl = uploadResult.Data;
                        Snackbar.Add("Captura estilo Post (1080x1360) generada con éxito.", Severity.Success);
                    }
                    else
                    {
                        Snackbar.Add("Se capturó el emprendimiento pero hubo error al subirla: " + uploadResult.Message, Severity.Warning);
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
            if (EmprendimientoSeleccionado == null || string.IsNullOrWhiteSpace(Informacion))
            {
                Snackbar.Add("Selecciona un emprendimiento para mejorar el texto.", Severity.Warning);
                return;
            }

            Mejorando = true;
            var result = await Publicaciones.MejorarEmprendimientoConIa(
                EmprendimientoSeleccionado.iIdEmpresa,
                EmprendimientoSeleccionado.vchNombreComercial ?? string.Empty,
                Informacion);
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
            if (!PuedePublicar || string.IsNullOrWhiteSpace(ImagenEfectiva))
            {
                Snackbar.Add("Elige una imagen y genera el texto de la publicación.", Severity.Warning);
                return;
            }

            Publicando = true;
            var imagenParaPublicar = ImagenEfectiva!;
            var result = await Publicaciones.PublicarFacebook(Mensaje.Trim(), imagenParaPublicar.Trim());
            Publicando = false;

            if (result.IsSuccess)
            {
                Snackbar.Add("El emprendimiento se envió a Facebook con éxito.", Severity.Success);
            }
            else
            {
                Snackbar.Add(string.IsNullOrWhiteSpace(result.Message) ? "No se pudo publicar el emprendimiento." : result.Message, Severity.Error);
            }
        }
    }
}
