using Microsoft.AspNetCore.Components;
using SantiagoConectaIA.PWA.Shared.Workspace;
using SantiagoConectaIA.PWA.Areas.EmpresasArea.Utiles;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SantiagoConectaIA.PWA.Areas.EmpresasArea.Components
{
    public partial class FormEmpresa : EngramaWorkspaceComponent
    {
        [Inject] public MainEmpresas Data { get; set; }
        
        // Evento para notificar al GridEmpresas que se guardó algo
        [Parameter] public EventCallback OnSuccess { get; set; }

        protected override async Task OnInitializedAsync()
        {
            await base.OnInitializedAsync();
            if (Data.LstCatalogos.Count == 0)
            {
                await Data.PostGetCatalogos();
            }
            if (Data.RegistroSeleccionado != null && Data.RegistroSeleccionado.iIdEmpresa > 0)
            {
                // Cargar datos dependientes al abrir una empresa existente
                await Data.PostGetUbicaciones();
                await Data.PostGetRedesSociales();
                await Data.PostGetCategorias();
            }
        }

        private async Task Submit()
        {
            var result = await Data.PostSaveRegistro();
            ShowSnake(result);

            if (result.bResult)
            {
                // Cambiar el estado a solo lectura tras guardar exitosamente
                EstadoControl = TipoEstadoControl.Lectura;
                
                SetNombreTab($"Empresa: {Data.RegistroSeleccionado.vchNombreComercial}");
                
                // Actualizar los botones del menú superior
                TriggerMenuUpdate();
                
                await OnSuccess.InvokeAsync();
            }
        }

        // Sobrescribir este método para definir los botones superiores según el Estado de Control
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
                    Action = EventCallback.Factory.Create(this, () => {
                        EstadoControl = TipoEstadoControl.Edicion;
                        TriggerMenuUpdate(); // Refrescar menú para mostrar botón Guardar
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
