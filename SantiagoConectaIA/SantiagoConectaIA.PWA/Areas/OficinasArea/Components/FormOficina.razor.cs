using Microsoft.AspNetCore.Components;
using MudBlazor;
using SantiagoConectaIA.PWA.Areas.OficinasArea.Utiles;
using SantiagoConectaIA.PWA.Shared.Workspace;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SantiagoConectaIA.PWA.Areas.OficinasArea.Components
{
    public partial class FormOficina : EngramaWorkspaceComponent
    {
        [Parameter] public MainOficinas Data { get; set; }
        [Parameter] public EventCallback OnSuccess { get; set; }

        private async Task OnSubmit()
        {
            Loading.Show();
            var result = await Data.PostSaveOficina();
            ShowSnake(result);
            if (result.bResult)
            {
                EstadoControl = TipoEstadoControl.Lectura;
                SetNombreTab($"Oficina: {Data.OficinaSelected.vchNombre}");
                TriggerMenuUpdate();
                StateHasChanged();
                await OnSuccess.InvokeAsync();
            }
            Loading.Hide();
        }

        protected override List<MenuItemModel> GetMenuItems()
        {
            var list = new List<MenuItemModel>();

            if (EstadoControl == TipoEstadoControl.Lectura)
            {
                list.Add(new MenuItemModel
                {
                    Text = "Editar",
                    Icon = Icons.Material.Filled.Edit,
                    Color = Color.Warning,
                    Action = EventCallback.Factory.Create(this, () => {
                        EstadoControl = TipoEstadoControl.Edicion;
                        TriggerMenuUpdate();
                        StateHasChanged();
                    })
                });
            }
            else
            {
                list.Add(new MenuItemModel
                {
                    Text = "Guardar",
                    Icon = Icons.Material.Filled.Save,
                    Color = Color.Success,
                    Action = EventCallback.Factory.Create(this, OnSubmit)
                });
            }

            list.Add(new MenuItemModel
            {
                Text = "Cerrar",
                Icon = Icons.Material.Filled.Close,
                Color = Color.Error,
                Action = EventCallback.Factory.Create(this, () => WorkspaceService.SetCloseCurrentTab(true))
            });

            return list;
        }
    }
}
