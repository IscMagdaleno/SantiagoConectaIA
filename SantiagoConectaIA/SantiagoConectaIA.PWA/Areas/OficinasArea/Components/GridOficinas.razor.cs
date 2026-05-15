using Microsoft.AspNetCore.Components;
using MudBlazor;
using SantiagoConectaIA.PWA.Areas.OficinasArea.Utiles;
using SantiagoConectaIA.PWA.Shared.Workspace;
using SantiagoConectaIA.Share.Objects.OficinasModule;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SantiagoConectaIA.PWA.Areas.OficinasArea.Components
{
    public partial class GridOficinas : EngramaWorkspaceComponent
    {
        [Inject] public MainOficinas Data { get; set; }

        public string filtroTexto { get; set; } = "";
        private List<Oficina> listaFiltrada;

        protected override async Task OnInitializedAsync()
        {
            await Data.PostGetOficinas();
            ActualizarListaFiltrada();
        }

        private void OnClickShowForm()
        {
            Data.OficinaSelected = new Oficina();
            AbrirFormularioOficina("Nueva Oficina", TipoEstadoControl.Alta);
        }

        private void OnViewOficina(Oficina oficina)
        {
            Data.OficinaSelected = oficina;
            AbrirFormularioOficina($"Oficina: {oficina.vchNombre}", TipoEstadoControl.Lectura);
        }

        private void OnEditOficina(Oficina oficina)
        {
            Data.OficinaSelected = oficina;
            AbrirFormularioOficina($"Editar: {oficina.vchNombre}", TipoEstadoControl.Edicion);
        }

        private void AbrirFormularioOficina(string titulo, TipoEstadoControl estado)
        {
            var type = typeof(FormOficina);
            var tab = new WorkspaceTab
            {
                Icono = Icons.Material.Filled.Business,
                Text = titulo,
                TipoControl = type,
                Repetir = true,
                EstadoControl = estado
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
                        if (baseComponent is FormOficina form)
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

        private async Task OnDeleteOficina(Oficina oficina)
        {
            await Data.PostGetOficinas();
            ActualizarListaFiltrada();
            StateHasChanged();
        }

        private void OnSuccess()
        {
            Data.PostGetOficinas().ContinueWith(_ => {
                ActualizarListaFiltrada();
                InvokeAsync(StateHasChanged);
            });
        }

        private void CuandoCambiaTexto(string nuevoTexto)
        {
            filtroTexto = nuevoTexto;
            ActualizarListaFiltrada();
            StateHasChanged();
        }

        private void ActualizarListaFiltrada()
        {
            if (string.IsNullOrEmpty(filtroTexto))
            {
                listaFiltrada = Data.LstOficinas.ToList();
            }
            else
            {
                listaFiltrada = Data.LstOficinas
                    .Where(o => 
                        (o.vchNombre != null && o.vchNombre.Contains(filtroTexto, System.StringComparison.OrdinalIgnoreCase)) ||
                        (o.vchDireccion != null && o.vchDireccion.Contains(filtroTexto, System.StringComparison.OrdinalIgnoreCase)) ||
                        (o.vchTelefono != null && o.vchTelefono.Contains(filtroTexto, System.StringComparison.OrdinalIgnoreCase))
                    )
                    .ToList();
            }
        }

        protected override List<MenuItemModel> GetMenuItems()
        {
            return new List<MenuItemModel>
            {
                new MenuItemModel
                {
                    Text = "Agregar Oficina",
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
                        await Data.PostGetOficinas();
                        ActualizarListaFiltrada();
                        StateHasChanged();
                        Loading.Hide();
                    })
                }
            };
        }
    }
}
