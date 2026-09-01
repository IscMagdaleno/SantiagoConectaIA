using Microsoft.AspNetCore.Components;
using MudBlazor;
using SantiagoConectaIA.PWA.Areas.EmpresasArea.Utiles;
using SantiagoConectaIA.PWA.Shared.Workspace;
using SantiagoConectaIA.PWA.Shared.Common;
using System.Collections.Generic;
using System.Threading.Tasks;
using SantiagoConectaIA.Share.Objects.EmpresasModulo;

namespace SantiagoConectaIA.PWA.Areas.EmpresasArea.Components
{
    public partial class GridEmpresas : EngramaWorkspaceComponent
    {
        [Inject] public MainEmpresas Data { get; set; }
        public string filtroTexto { get; set; } = string.Empty;
        public List<Empresa> listaFiltrada { get; set; } = new List<Empresa>();

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
                listaFiltrada = Data.LstRegistros?.ToList() ?? new List<Empresa>();
                return;
            }
            listaFiltrada = Data.LstRegistros?
                .Where(e => (e.vchNombreComercial != null && e.vchNombreComercial.ToLower().Contains(filtroTexto.ToLower())) ||
                            (e.vchSlogan != null && e.vchSlogan.ToLower().Contains(filtroTexto.ToLower())))
                .ToList() ?? new List<Empresa>();
        }

        private Empresa CloneEmpresa(Empresa source)
        {
            if (source == null) return new Empresa();
            var json = System.Text.Json.JsonSerializer.Serialize(source);
            return System.Text.Json.JsonSerializer.Deserialize<Empresa>(json) ?? new Empresa();
        }

        // Abrir pestaña para Crear
        private void OnClickShowForm()
        {
            var newEmpresa = new Empresa(); 
            var type = typeof(FormEmpresa);
            var tab = new WorkspaceTab
            {
                Icono = Icons.Material.Filled.AddBox,
                Text = "Nueva Empresa",
                TipoControl = type,
                Repetir = true,
                EstadoControl = TipoEstadoControl.Alta
            };

            tab.Componente = builder =>
            {
                builder.OpenComponent(0, type);
                builder.AddAttribute(1, "Model", newEmpresa);
                builder.AddComponentReferenceCapture(2, instance =>
                {
                    if (instance is EngramaWorkspaceComponent baseComponent)
                    {
                        tab.InstanciaComponente = baseComponent;
                        baseComponent.IconoBase = tab.Icono ?? "";
                        baseComponent.EstadoControl = tab.EstadoControl;
                        if (baseComponent is FormEmpresa form)
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

        // Abrir pestaña para Ver
        protected void OnViewRegistro(Empresa registro)
        {
            var empresaModel = CloneEmpresa(registro);
            var type = typeof(FormEmpresa);
            var tab = new WorkspaceTab
            {
                Icono = Icons.Material.Filled.Visibility,
                Text = $"Ver Empresa: {registro.vchNombreComercial}",
                TipoControl = type,
                Repetir = true,
                EstadoControl = TipoEstadoControl.Lectura
            };

            tab.Componente = builder =>
            {
                builder.OpenComponent(0, type);
                builder.AddAttribute(1, "Model", empresaModel);
                builder.AddComponentReferenceCapture(2, instance =>
                {
                    if (instance is EngramaWorkspaceComponent baseComponent)
                    {
                        tab.InstanciaComponente = baseComponent;
                        baseComponent.IconoBase = tab.Icono ?? "";
                        baseComponent.EstadoControl = tab.EstadoControl;
                        if (baseComponent is FormEmpresa form)
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

        // Abrir pestaña para Editar
        protected void OnEditRegistro(Empresa registro)
        {
            var empresaModel = CloneEmpresa(registro);
            var type = typeof(FormEmpresa);
            var tab = new WorkspaceTab
            {
                Icono = Icons.Material.Filled.Edit,
                Text = $"Editar: {registro.vchNombreComercial}",
                TipoControl = type,
                Repetir = true,
                EstadoControl = TipoEstadoControl.Edicion
            };

            tab.Componente = builder =>
            {
                builder.OpenComponent(0, type);
                builder.AddAttribute(1, "Model", empresaModel);
                builder.AddComponentReferenceCapture(2, instance =>
                {
                    if (instance is EngramaWorkspaceComponent baseComponent)
                    {
                        tab.InstanciaComponente = baseComponent;
                        baseComponent.IconoBase = tab.Icono ?? "";
                        baseComponent.EstadoControl = tab.EstadoControl;
                        if (baseComponent is FormEmpresa form)
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

        // Menú contextual principal del componente
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
