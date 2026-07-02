using Microsoft.AspNetCore.Components;
using SantiagoConectaIA.PWA.Shared.Workspace;
using SantiagoConectaIA.PWA.Areas.EventosArea.Utiles;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SantiagoConectaIA.PWA.Areas.EventosArea.Components
{
    public partial class FormEvento : EngramaWorkspaceComponent
    {
        [Inject] public MainEventos Data { get; set; }

        [Parameter] public EventCallback OnSuccess { get; set; }

        private string fechaInicioStr
        {
            get => Data.RegistroSeleccionado.dtFechaInicio.ToString("yyyy-MM-ddTHH:mm");
            set
            {
                if (DateTime.TryParse(value, out var parsed))
                {
                    Data.RegistroSeleccionado.dtFechaInicio = parsed;
                }
            }
        }

        private string fechaFinStr
        {
            get => Data.RegistroSeleccionado.dtFechaFin?.ToString("yyyy-MM-ddTHH:mm") ?? "";
            set
            {
                if (DateTime.TryParse(value, out var parsed))
                {
                    Data.RegistroSeleccionado.dtFechaFin = parsed;
                }
                else
                {
                    Data.RegistroSeleccionado.dtFechaFin = null;
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
            if (Data.RegistroSeleccionado != null && Data.RegistroSeleccionado.iIdEvento > 0)
            {
                await Data.PostGetDetalle(Data.RegistroSeleccionado.iIdEvento);
                await Data.PostGetSucursales(Data.RegistroSeleccionado.iIdEvento);
            }
        }

        private async Task Submit()
        {
            var result = await Data.PostSaveRegistro();
            ShowSnake(result);

            if (result.bResult)
            {
                EstadoControl = TipoEstadoControl.Lectura;
                SetNombreTab($"Evento: {Data.RegistroSeleccionado.vchNombre}");
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
