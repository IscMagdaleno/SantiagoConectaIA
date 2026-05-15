using EngramaCoreStandar.Mapper;
using EngramaCoreStandar.Servicios;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using SantiagoConectaIA.PWA.Areas.NoticiasArea.Utiles;
using SantiagoConectaIA.PWA.Shared.Common;
using SantiagoConectaIA.Share.Objects.NoticiasModule;
using System.Threading.Tasks;

using SantiagoConectaIA.PWA.Shared.Workspace;

namespace SantiagoConectaIA.PWA.Areas.NoticiasArea.Components
{
    public partial class GridNoticias : EngramaWorkspaceComponent
    {
        [Inject] public MainNoticias Data { get; set; }
        
        // UI State
        public string filtroTexto { get; set; } = "";

        public IEnumerable<Noticia> NoticiasFiltradas => 
            string.IsNullOrWhiteSpace(filtroTexto) 
            ? Data.LstNoticias 
            : Data.LstNoticias.Where(n => (n.vchTitulo?.Contains(filtroTexto, StringComparison.OrdinalIgnoreCase) ?? false) || 
                                          (n.nvchContenido?.Contains(filtroTexto, StringComparison.OrdinalIgnoreCase) ?? false));

        protected override async Task OnInitializedAsync()
        {
            await Data.PostGetNoticias();
        }

        // Show Create Form
        private void OnClickShowForm()
        {
            Data.NoticiaSelected = new Noticia();
            var type = typeof(FormNoticias);
            var tab = new WorkspaceTab
            {
                Icono = Icons.Material.Filled.Article,
                Text = "Nueva Noticia",
                TipoControl = type,
                Repetir = false,
                EstadoControl = TipoEstadoControl.Alta
            };

            tab.Componente = builder =>
            {
                builder.OpenComponent(0, type);
                builder.AddAttribute(1, "Data", Data);
                builder.AddComponentReferenceCapture(2, instance =>
                {
                    if (instance is EngramaWorkspaceComponent baseComponent)
                    {
                        tab.InstanciaComponente = baseComponent;
                        baseComponent.IconoBase = tab.Icono ?? "";
                        baseComponent.EstadoControl = tab.EstadoControl;
                        if (baseComponent is FormNoticias form)
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

        // View Item
        private void OnViewNoticia(Noticia noticia)
        {
            Data.NoticiaSelected = noticia;
            var type = typeof(FormNoticias);
            var tab = new WorkspaceTab
            {
                Icono = Icons.Material.Filled.Visibility,
                Text = $"Noticia {noticia.iIdNoticia}",
                TipoControl = type,
                Repetir = true,
                EstadoControl = TipoEstadoControl.Lectura
            };

            tab.Componente = builder =>
            {
                builder.OpenComponent(0, type);
                builder.AddAttribute(1, "Data", Data);
                builder.AddComponentReferenceCapture(2, instance =>
                {
                    if (instance is EngramaWorkspaceComponent baseComponent)
                    {
                        tab.InstanciaComponente = baseComponent;
                        baseComponent.IconoBase = tab.Icono ?? "";
                        baseComponent.EstadoControl = tab.EstadoControl;
                        if (baseComponent is FormNoticias form)
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

        // Edit Item
        private void OnEditNoticia(Noticia noticia)
        {
            Data.NoticiaSelected = noticia;
            var type = typeof(FormNoticias);
            var tab = new WorkspaceTab
            {
                Icono = Icons.Material.Filled.Edit,
                Text = $"Noticia {noticia.iIdNoticia}",
                TipoControl = type,
                Repetir = true,
                EstadoControl = TipoEstadoControl.Edicion
            };

            tab.Componente = builder =>
            {
                builder.OpenComponent(0, type);
                builder.AddAttribute(1, "Data", Data);
                builder.AddComponentReferenceCapture(2, instance =>
                {
                    if (instance is EngramaWorkspaceComponent baseComponent)
                    {
                        tab.InstanciaComponente = baseComponent;
                        baseComponent.IconoBase = tab.Icono ?? "";
                        baseComponent.EstadoControl = tab.EstadoControl;
                        if (baseComponent is FormNoticias form)
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

        private async Task OnDeleteNoticia(Noticia noticia)
        {
            // Soft delete logic will be in NoticiaCardComponent or handled here
            await Data.PostGetNoticias();
            StateHasChanged();
        }

        protected override List<MenuItemModel> GetMenuItems()
        {
            return new List<MenuItemModel>
            {
                new MenuItemModel
                {
                    Text = "Agregar Noticia",
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
                        Loading.Show();
                        await Data.PostGetNoticias();
                        StateHasChanged();
                        Loading.Hide();
                    })
                }
            };
        }

        public async Task OnSuccess()
        {
            await Data.PostGetNoticias();
            StateHasChanged();
        }
    }
}
