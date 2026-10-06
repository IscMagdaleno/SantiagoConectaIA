using Microsoft.AspNetCore.Components;
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

        public List<Empresa> LstEmprendimientos { get; set; } = new();
        public string FiltroTexto { get; set; } = string.Empty;
        public bool Cargando { get; set; } = true;
        public bool CargandoDetalle { get; set; }
        public bool Mejorando { get; set; }
        public bool Publicando { get; set; }

        public PublicacionAutomaticaEmprendimientos? Automatica { get; set; }
        public bool GuardandoAutomatica { get; set; }
        public bool PublicandoSiguiente { get; set; }

        public Empresa? EmprendimientoSeleccionado { get; set; }
        public PostSaveEmprendimientoFull? Detalle { get; set; }
        public string Informacion { get; set; } = string.Empty;
        public string Mensaje { get; set; } = string.Empty;
        public List<string> Imagenes { get; set; } = new();
        public string? ImagenSeleccionada { get; set; }
        public bool EditandoImagen { get; set; }

        public bool Ocupado => Mejorando || Publicando || EditandoImagen;
        public bool PuedeMejorar => EmprendimientoSeleccionado != null && !CargandoDetalle && !string.IsNullOrWhiteSpace(Informacion) && !Ocupado;
        public bool PuedeEditarImagen => !string.IsNullOrWhiteSpace(ImagenSeleccionada) && !Ocupado;
        public bool PuedePublicar => EmprendimientoSeleccionado != null && !string.IsNullOrWhiteSpace(ImagenSeleccionada) && !string.IsNullOrWhiteSpace(Mensaje) && !Ocupado;

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

        public async Task PublicarSiguiente()
        {
            var siguiente = Automatica != null && !string.IsNullOrWhiteSpace(Automatica.vchSiguienteEmpresa)
                ? $"#{Automatica.iIdSiguienteEmpresa} {Automatica.vchSiguienteEmpresa}"
                : "el siguiente emprendimiento";

            var confirmar = await DialogService.ShowMessageBox(
                "Publicar siguiente emprendimiento",
                $"Se publicará en Facebook: {siguiente}. Después la rotación avanza al que sigue. ¿Continuar?",
                yesText: "Publicar",
                cancelText: "Cancelar");
            if (confirmar != true)
            {
                return;
            }

            PublicandoSiguiente = true;
            var result = await Publicaciones.PublicarSiguienteEmprendimiento();
            PublicandoSiguiente = false;

            // El API devuelve el estado actualizado (cuál sigue) aun cuando no publica
            if (result.Data != null)
            {
                Automatica = result.Data;
            }

            Snackbar.Add(
                string.IsNullOrWhiteSpace(result.Message) ? "No se pudo publicar el emprendimiento." : result.Message,
                result.IsSuccess ? Severity.Success : Severity.Warning);
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
        }

        public void ElegirImagen(string url)
        {
            ImagenSeleccionada = url;
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
            if (!PuedePublicar || string.IsNullOrWhiteSpace(ImagenSeleccionada))
            {
                Snackbar.Add("Elige una imagen y genera el texto de la publicación.", Severity.Warning);
                return;
            }

            Publicando = true;
            var result = await Publicaciones.PublicarFacebook(Mensaje.Trim(), ImagenSeleccionada.Trim());
            Publicando = false;

            if (result.IsSuccess)
            {
                Snackbar.Add("El emprendimiento se envió a Facebook.", Severity.Success);
            }
            else
            {
                Snackbar.Add(string.IsNullOrWhiteSpace(result.Message) ? "No se pudo publicar el emprendimiento." : result.Message, Severity.Error);
            }
        }
    }
}
