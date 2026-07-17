using Microsoft.AspNetCore.Components;
using MudBlazor;
using SantiagoConectaIA.PWA.Areas.EventosArea.Utiles;
using SantiagoConectaIA.PWA.Shared.Workspace;
using SantiagoConectaIA.PWA.Shared.Common;
using System.Collections.Generic;
using System.Threading.Tasks;
using SantiagoConectaIA.Share.Objects.EventosModulo;

namespace SantiagoConectaIA.PWA.Areas.EventosArea.Components
{
    public partial class GridEventos : EngramaWorkspaceComponent
    {
        [Inject] public MainEventos Data { get; set; }
        public string filtroTexto { get; set; } = string.Empty;
        public List<Evento> listaFiltrada { get; set; } = new List<Evento>();

        protected override async Task OnInitializedAsync()
        {
            await Data.PostGetRegistros();
            ActualizarListaFiltrada();
        }

        private async Task CuandoCambiaTexto(string nuevoTexto)
        {
            filtroTexto = nuevoTexto;
            ActualizarListaFiltrada();
            StateHasChanged();
        }

        private void ActualizarListaFiltrada()
        {
            if (string.IsNullOrWhiteSpace(filtroTexto))
            {
                listaFiltrada = Data.LstRegistros?.ToList() ?? new List<Evento>();
                return;
            }
            listaFiltrada = Data.LstRegistros?
                .Where(e => (e.vchNombre != null && e.vchNombre.ToLower().Contains(filtroTexto.ToLower())) ||
                            (e.vchLugar != null && e.vchLugar.ToLower().Contains(filtroTexto.ToLower())))
                .ToList() ?? new List<Evento>();
        }

        private void OnClickShowForm()
        {
            Data.RegistroSeleccionado = new Evento();
            var type = typeof(FormEvento);
            var tab = new WorkspaceTab
            {
                Icono = Icons.Material.Filled.AddBox,
                Text = "Nuevo Evento",
                TipoControl = type,
                Repetir = false,
                EstadoControl = TipoEstadoControl.Alta
            };

            tab.Componente = builder =>
            {
                builder.OpenComponent(0, type);
                builder.AddComponentReferenceCapture(1, instance =>
                {
                    if (instance is EngramaWorkspaceComponent baseComponent)
                    {
                        tab.InstanciaComponente = baseComponent;
                        baseComponent.IconoBase = tab.Icono ?? "";
                        baseComponent.EstadoControl = tab.EstadoControl;
                        if (baseComponent is FormEvento form)
                        {
                            form.OnSuccess = EventCallback.Factory.Create(this, OnSuccess);
                        }
                        baseComponent.TriggerMenuUpdate();
                    }
                });
                builder.CloseComponent();
            };

            AgregarTab(tab);
        }

        protected void OnViewRegistro(Evento registro)
        {
            Data.RegistroSeleccionado = registro;
            var type = typeof(FormEvento);
            var tab = new WorkspaceTab
            {
                Icono = Icons.Material.Filled.Visibility,
                Text = "Ver Evento",
                TipoControl = type,
                Repetir = true,
                EstadoControl = TipoEstadoControl.Lectura
            };

            tab.Componente = builder =>
            {
                builder.OpenComponent(0, type);
                builder.AddComponentReferenceCapture(1, instance =>
                {
                    if (instance is EngramaWorkspaceComponent baseComponent)
                    {
                        tab.InstanciaComponente = baseComponent;
                        baseComponent.IconoBase = tab.Icono ?? "";
                        baseComponent.EstadoControl = tab.EstadoControl;
                        if (baseComponent is FormEvento form)
                        {
                            form.OnSuccess = EventCallback.Factory.Create(this, OnSuccess);
                        }
                        baseComponent.TriggerMenuUpdate();
                    }
                });
                builder.CloseComponent();
            };

            AgregarTab(tab);
        }

        protected void OnEditRegistro(Evento registro)
        {
            Data.RegistroSeleccionado = registro;
            var type = typeof(FormEvento);
            var tab = new WorkspaceTab
            {
                Icono = Icons.Material.Filled.Edit,
                Text = "Editar Evento",
                TipoControl = type,
                Repetir = true,
                EstadoControl = TipoEstadoControl.Edicion
            };

            tab.Componente = builder =>
            {
                builder.OpenComponent(0, type);
                builder.AddComponentReferenceCapture(1, instance =>
                {
                    if (instance is EngramaWorkspaceComponent baseComponent)
                    {
                        tab.InstanciaComponente = baseComponent;
                        baseComponent.IconoBase = tab.Icono ?? "";
                        baseComponent.EstadoControl = tab.EstadoControl;
                        if (baseComponent is FormEvento form)
                        {
                            form.OnSuccess = EventCallback.Factory.Create(this, OnSuccess);
                        }
                        baseComponent.TriggerMenuUpdate();
                    }
                });
                builder.CloseComponent();
            };

            AgregarTab(tab);
        }

        protected override List<MenuItemModel> GetMenuItems()
        {
            return new List<MenuItemModel>
            {
                new MenuItemModel
                {
                    Text = "Agregar",
                    Icon = Icons.Material.Filled.Add,
                    Color = Color.Success,
                    Action = EventCallback.Factory.Create(this, OnClickShowForm)
                },
                new MenuItemModel
                {
                    Text = "Actualizar",
                    Icon = Icons.Material.Filled.Refresh,
                    Color = Color.Info,
                    Action = EventCallback.Factory.Create(this, async () => {
                        await Data.PostGetRegistros();
                        ActualizarListaFiltrada();
                        StateHasChanged();
                    })
                }
            };
        }

        public async Task OnSuccess()
        {
            await Data.PostGetRegistros();
            ActualizarListaFiltrada();
            StateHasChanged();
        }
    }
}
