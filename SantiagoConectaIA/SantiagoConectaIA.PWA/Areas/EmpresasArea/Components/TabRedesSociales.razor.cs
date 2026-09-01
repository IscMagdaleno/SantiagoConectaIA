using Microsoft.AspNetCore.Components;
using SantiagoConectaIA.PWA.Areas.EmpresasArea.Utiles;
using System.Threading.Tasks;
using MudBlazor;
using SantiagoConectaIA.Share.Objects.EmpresasModulo;

namespace SantiagoConectaIA.PWA.Areas.EmpresasArea.Components
{
    public partial class TabRedesSociales : ComponentBase
    {
        [Inject] public MainEmpresas Data { get; set; }
        [Inject] public ISnackbar Snackbar { get; set; }
        [Parameter] public Empresa EmpresaModel { get; set; } = default!;

        private bool dialogVisible = false;
        private DialogOptions dialogOptions = new() { MaxWidth = MaxWidth.Small, FullWidth = true, CloseButton = true };
        private EmpresaRedSocial redActual = new();

        protected override async Task OnInitializedAsync()
        {
            await base.OnInitializedAsync();
            var targetId = EmpresaModel?.iIdEmpresa ?? Data.RegistroSeleccionado.iIdEmpresa;
            if (targetId > 0)
            {
                await Data.PostGetRedesSociales(targetId);
            }
        }

        private void AbrirDialogoNuevaRed()
        {
            redActual = new EmpresaRedSocial
            {
                iIdEmpresa = EmpresaModel?.iIdEmpresa ?? Data.RegistroSeleccionado.iIdEmpresa,
                bActivo = true
            };
            dialogVisible = true;
        }

        private void EditarRed(EmpresaRedSocial red)
        {
            redActual = new EmpresaRedSocial
            {
                iIdRedSocial = red.iIdRedSocial,
                iIdEmpresa = red.iIdEmpresa,
                vchPlataforma = red.vchPlataforma,
                vchUrl = red.vchUrl,
                bActivo = red.bActivo
            };
            dialogVisible = true;
        }

        private void CerrarDialogo()
        {
            dialogVisible = false;
        }

        private async Task GuardarRed()
        {
            if (string.IsNullOrWhiteSpace(redActual.vchPlataforma) || string.IsNullOrWhiteSpace(redActual.vchUrl))
            {
                Snackbar.Add("La plataforma y la URL son requeridas.", Severity.Warning);
                return;
            }

            var result = await Data.PostSaveRedSocial(redActual);
            if (result.bResult)
            {
                var targetId = EmpresaModel?.iIdEmpresa ?? Data.RegistroSeleccionado.iIdEmpresa;
                await Data.PostGetRedesSociales(targetId);
                Snackbar.Add("Red social guardada exitosamente.", Severity.Success);
                dialogVisible = false;
            }
            else
            {
                Snackbar.Add(result.vchMessage, Severity.Error);
            }
        }
    }
}
