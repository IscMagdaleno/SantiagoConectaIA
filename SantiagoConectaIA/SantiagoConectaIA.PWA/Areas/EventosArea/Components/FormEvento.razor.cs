using Microsoft.AspNetCore.Components;
using SantiagoConectaIA.PWA.Shared.Workspace;
using SantiagoConectaIA.PWA.Areas.EventosArea.Utiles;
using SantiagoConectaIA.Share.Objects.EventosModulo;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SantiagoConectaIA.PWA.Areas.EventosArea.Components
{
    public partial class FormEvento : EngramaWorkspaceComponent
    {
        [Inject] public MainEventos Data { get; set; }
        [Parameter] public Evento Model { get; set; } = new();

        [Parameter] public EventCallback OnSuccess { get; set; }

        private DateTime? FechaInicioDate
        {
            get => Model.dtFechaInicio.Date;
            set
            {
                if (value.HasValue)
                {
                    Model.dtFechaInicio = value.Value.Add(Model.dtFechaInicio.TimeOfDay);
                }
            }
        }

        private TimeSpan? FechaInicioTime
        {
            get => Model.dtFechaInicio.TimeOfDay;
            set
            {
                if (value.HasValue)
                {
                    Model.dtFechaInicio = Model.dtFechaInicio.Date.Add(value.Value);
                }
            }
        }

        private DateTime? FechaFinDate
        {
            get => Model.dtFechaFin?.Date;
            set
            {
                if (value.HasValue)
                {
                    var time = Model.dtFechaFin?.TimeOfDay ?? TimeSpan.Zero;
                    Model.dtFechaFin = value.Value.Add(time);
                }
                else
                {
                    Model.dtFechaFin = null;
                }
            }
        }

        private TimeSpan? FechaFinTime
        {
            get => Model.dtFechaFin?.TimeOfDay;
            set
            {
                if (value.HasValue)
                {
                    var date = Model.dtFechaFin?.Date ?? DateTime.Today;
                    Model.dtFechaFin = date.Add(value.Value);
                }
            }
        }

        protected override async Task OnInitializedAsync()
        {
            await base.OnInitializedAsync();
            if (Data.LstCategorias.Count == 0)
            {
                await Data.PostGetCategorias();
            }
            if (Model != null)
            {
                if (Model.dtFechaInicio == DateTime.MinValue)
                {
                    Model.dtFechaInicio = DateTime.Now;
                }
            }
        }

        private bool _isScanningIA;

        private async Task EscanearFlyerConIA(Microsoft.AspNetCore.Components.Forms.IBrowserFile file)
        {
            if (file == null) return;

            try
            {
                _isScanningIA = true;
                StateHasChanged();

                var response = await Data.PostEscanearEventoConIA(file);
                if (response != null && response.IsSuccess && response.Data != null)
                {
                    var extraido = response.Data;
                    
                    if (!string.IsNullOrWhiteSpace(extraido.vchNombre)) Model.vchNombre = extraido.vchNombre;
                    if (!string.IsNullOrWhiteSpace(extraido.nvchDescripcion)) Model.nvchDescripcion = extraido.nvchDescripcion;
                    if (extraido.dtFechaInicio != DateTime.MinValue) Model.dtFechaInicio = extraido.dtFechaInicio;
                    if (extraido.dtFechaFin.HasValue) Model.dtFechaFin = extraido.dtFechaFin;
                    if (!string.IsNullOrWhiteSpace(extraido.vchLugar)) Model.vchLugar = extraido.vchLugar;
                    if (!string.IsNullOrWhiteSpace(extraido.vchDireccion)) Model.vchDireccion = extraido.vchDireccion;
                    if (!string.IsNullOrWhiteSpace(extraido.vchCostoTexto)) Model.vchCostoTexto = extraido.vchCostoTexto;
                    if (!string.IsNullOrWhiteSpace(extraido.vchOrganizador)) Model.vchOrganizador = extraido.vchOrganizador;
                    if (!string.IsNullOrWhiteSpace(extraido.vchTelefono)) Model.vchTelefono = extraido.vchTelefono;
                    if (!string.IsNullOrWhiteSpace(extraido.vchCorreo)) Model.vchCorreo = extraido.vchCorreo;
                    if (!string.IsNullOrWhiteSpace(extraido.vchUrlOficial)) Model.vchUrlOficial = extraido.vchUrlOficial;
                    if (!string.IsNullOrWhiteSpace(extraido.vchImagenPortada)) Model.vchImagenPortada = extraido.vchImagenPortada;
                    if (extraido.flLatitud != 0) Model.flLatitud = extraido.flLatitud;
                    if (extraido.flLongitud != 0) Model.flLongitud = extraido.flLongitud;

                    SetNombreTab($"Evento: {Model.vchNombre}");
                    TriggerMenuUpdate();
                    Snackbar.Add("¡Datos extraídos con éxito y portada asignada!", MudBlazor.Severity.Success);
                }
                else
                {
                    Snackbar.Add(response?.Message ?? "No se pudo extraer la información del flyer.", MudBlazor.Severity.Error);
                }
            }
            catch (Exception ex)
            {
                Snackbar.Add($"Error al analizar el cartel: {ex.Message}", MudBlazor.Severity.Error);
            }
            finally
            {
                _isScanningIA = false;
                StateHasChanged();
            }
        }

        private async Task Submit()
        {
            var result = await Data.PostSaveRegistro(Model);
            ShowSnake(result);

            if (result.bResult)
            {
                EstadoControl = TipoEstadoControl.Lectura;
                SetNombreTab($"Evento: {Model.vchNombre}");
                TriggerMenuUpdate();
                await OnSuccess.InvokeAsync();
            }
        }

        protected override List<MenuItemModel> GetMenuItems()
        {
            var items = new List<MenuItemModel>();

            if (EstadoControl == TipoEstadoControl.Lectura)
            {
                items.Add(new MenuItemModel
                {
                    Text = "Editar",
                    Icon = MudBlazor.Icons.Material.Filled.Edit,
                    Color = MudBlazor.Color.Primary,
                    Action = EventCallback.Factory.Create(this, () =>
                    {
                        EstadoControl = TipoEstadoControl.Edicion;
                        TriggerMenuUpdate();
                    })
                });
            }
            else
            {
                items.Add(new MenuItemModel
                {
                    Text = "Guardar",
                    Icon = MudBlazor.Icons.Material.Filled.Save,
                    Color = MudBlazor.Color.Success,
                    Action = EventCallback.Factory.Create(this, Submit)
                });
            }

            items.Add(new MenuItemModel
            {
                Text = "Cerrar",
                Icon = MudBlazor.Icons.Material.Filled.Close,
                Color = MudBlazor.Color.Error,
                Action = EventCallback.Factory.Create(this, CerrarTab)
            });

            return items;
        }
    }
}
