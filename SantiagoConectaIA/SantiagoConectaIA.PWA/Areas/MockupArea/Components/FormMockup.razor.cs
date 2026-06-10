using Microsoft.AspNetCore.Components;
using SantiagoConectaIA.PWA.Shared.Workspace;
using SantiagoConectaIA.PWA.Areas.MockupArea.Utiles;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SantiagoConectaIA.PWA.Areas.MockupArea.Components
{
    /// <summary>
    /// Code-Behind del formulario. Hereda de EngramaWorkspaceComponent para integrarse con las pestañas.
    /// </summary>
    public partial class FormMockup : EngramaWorkspaceComponent
    {
        // Inyectamos o pasamos como parámetro la clase Main creada en la carpeta Utiles
        [Parameter] public MainMockup Data { get; set; }
        
        // Evento opcional para notificar a componentes padre cuando se guarda con éxito
        [Parameter] public EventCallback OnSuccess { get; set; }

        private async Task Submit()
        {
            var result = await Data.PostSaveRegistro();
            ShowSnake(result); // Muestra notificación global

            if (result.bResult)
            {
                // Cambiar el estado a solo lectura tras guardar exitosamente
                EstadoControl = TipoEstadoControl.Lectura;
                
                // Actualizar el título de la pestaña (TODO: Reemplazar con una propiedad real de tu modelo)
                SetNombreTab($"Registro Guardado");
                
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
                // Modo Vista: Mostrar botón "Editar"
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
                // Modo Edición / Alta: Mostrar botón "Guardar"
                items.Add(new MenuItemModel
                {
                    Text = "Guardar",
                    Icon = MudBlazor.Icons.Material.Filled.Save,
                    Color = MudBlazor.Color.Success,
                    Action = EventCallback.Factory.Create(this, Submit)
                });
            }

            // Siempre mostrar el botón de cerrar la pestaña
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
