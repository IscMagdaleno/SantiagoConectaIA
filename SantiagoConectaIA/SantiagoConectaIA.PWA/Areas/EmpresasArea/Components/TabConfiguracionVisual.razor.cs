using Microsoft.AspNetCore.Components;
using SantiagoConectaIA.PWA.Areas.EmpresasArea.Utiles;
using SantiagoConectaIA.PWA.Shared.Workspace;
using MudBlazor;
using System.Threading.Tasks;

namespace SantiagoConectaIA.PWA.Areas.EmpresasArea.Components
{
    public partial class TabConfiguracionVisual : ComponentBase
    {
        [Inject] public MainEmpresas Data { get; set; }
        [Inject] public ISnackbar Snackbar { get; set; }

        [Parameter] public TipoEstadoControl EstadoControl { get; set; }

        protected override async Task OnInitializedAsync()
        {
            await base.OnInitializedAsync();
            // Cargar la configuración visual solo si la empresa ya está guardada y es válida
            if (Data.RegistroSeleccionado != null && Data.RegistroSeleccionado.iIdEmpresa > 0)
            {
                await Data.PostGetConfiguracionVisual();
            }
        }

        private async Task GuardarConfiguracion()
        {
            var result = await Data.PostSaveConfiguracionVisual();
            if (result.bResult)
            {
                Snackbar.Add("Configuración visual guardada exitosamente.", Severity.Success);
            }
            else
            {
                Snackbar.Add(result.vchMessage ?? "Ocurrió un error al guardar la configuración", Severity.Error);
            }
        }
    }
}
