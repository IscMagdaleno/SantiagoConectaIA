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

        [Parameter] public SantiagoConectaIA.Share.Objects.EmpresasModulo.Empresa EmpresaModel { get; set; } = default!;
        [Parameter] public TipoEstadoControl EstadoControl { get; set; }

        protected override async Task OnInitializedAsync()
        {
            await base.OnInitializedAsync();
            var targetId = EmpresaModel?.iIdEmpresa ?? Data.RegistroSeleccionado.iIdEmpresa;
            if (targetId > 0)
            {
                await Data.PostGetConfiguracionVisual(targetId);
            }
        }

        private async Task GuardarConfiguracion()
        {
            var targetId = EmpresaModel?.iIdEmpresa ?? Data.RegistroSeleccionado.iIdEmpresa;
            var result = await Data.PostSaveConfiguracionVisual(iIdEmpresa: targetId);
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
