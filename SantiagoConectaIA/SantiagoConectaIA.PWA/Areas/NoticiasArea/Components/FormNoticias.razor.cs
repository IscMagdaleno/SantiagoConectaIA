using Microsoft.AspNetCore.Components;
using SantiagoConectaIA.PWA.Areas.NoticiasArea.Utiles;
using SantiagoConectaIA.PWA.Shared.Workspace;

namespace SantiagoConectaIA.PWA.Areas.NoticiasArea.Components
{
    public partial class FormNoticias : EngramaWorkspaceComponent
    {
        [Parameter] public MainNoticias Data { get; set; }
        [Parameter] public EventCallback OnSuccess { get; set; }

        private async Task Submit()
        {
            var result = await Data.PostSaveNoticia();
            ShowSnake(result);
            if (result.bResult)
            {
                // Pasar a modo Lectura después de guardar
                EstadoControl = TipoEstadoControl.Lectura;
                
                // Actualizar el nombre del tab con el nuevo ID si era un alta
                SetNombreTab($"Noticia {Data.NoticiaSelected.vchTitulo}");
                
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
                    Action = EventCallback.Factory.Create(this, () => {
                        EstadoControl = TipoEstadoControl.Edicion;
                        TriggerMenuUpdate();
                    })
                });
                items.Add(new MenuItemModel
                {
                    Text = "Cerrar",
                    Icon = MudBlazor.Icons.Material.Filled.Close,
                    Color = MudBlazor.Color.Error,
                    Action = EventCallback.Factory.Create(this, CerrarTab)
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
                items.Add(new MenuItemModel
                {
                    Text = "Cerrar",
                    Icon = MudBlazor.Icons.Material.Filled.Close,
                    Color = MudBlazor.Color.Error,
                    Action = EventCallback.Factory.Create(this, CerrarTab)
                });
            }

            return items;
        }
    }
}
