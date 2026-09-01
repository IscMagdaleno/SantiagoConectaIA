using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using SantiagoConectaIA.PWA.Areas.NoticiasArea.Utiles;
using SantiagoConectaIA.PWA.Shared.Workspace;
using System.Threading.Tasks;

namespace SantiagoConectaIA.PWA.Areas.NoticiasArea.Components
{
    public partial class FormNoticias : EngramaWorkspaceComponent
    {
        [Parameter] public MainNoticias Data { get; set; } = default!;
        [Parameter] public SantiagoConectaIA.Share.Objects.NoticiasModule.Noticia Model { get; set; } = new();
        [Parameter] public EventCallback OnSuccess { get; set; }

        private async Task UploadPortada(IBrowserFile file)
        {
            if (file == null) return;
            var result = await Data.PostUploadPortada(file, Model?.vchTitulo);
            if (result.IsSuccess && result.Data != null)
            {
                Model.vchImagenPortada = result.Data.URL;
                StateHasChanged();
                ShowSnake(new EngramaCoreStandar.Dapper.Results.SeverityMessage(true, "Portada subida exitosamente"));
            }
            else
            {
                ShowSnake(new EngramaCoreStandar.Dapper.Results.SeverityMessage(false, result.Message ?? "Error al subir la portada"));
            }
        }

        private void EliminarPortada()
        {
            Model.vchImagenPortada = null;
            StateHasChanged();
        }

        private async Task Submit()
        {
            var result = await Data.PostSaveNoticia(Model);
            ShowSnake(result);
            if (result.bResult)
            {
                // Pasar a modo Lectura después de guardar
                EstadoControl = TipoEstadoControl.Lectura;
                
                // Actualizar el nombre del tab con el nuevo ID si era un alta
                SetNombreTab($"Noticia: {Model.vchTitulo}");
                
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
