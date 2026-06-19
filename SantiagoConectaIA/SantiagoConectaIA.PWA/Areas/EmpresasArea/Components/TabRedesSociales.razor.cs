using Microsoft.AspNetCore.Components;
using SantiagoConectaIA.PWA.Areas.EmpresasArea.Utiles;
using SantiagoConectaIA.Share.Objetos.EmpresasModulo;
using System.Threading.Tasks;
using MudBlazor;

namespace SantiagoConectaIA.PWA.Areas.EmpresasArea.Components
{
    public partial class TabRedesSociales : ComponentBase
    {
        [Inject] public MainEmpresas Data { get; set; }
        [Inject] public ISnackbar Snackbar { get; set; }

        private bool dialogVisible = false;
        private DialogOptions dialogOptions = new() { MaxWidth = MaxWidth.Small, FullWidth = true, CloseButton = true };
        private EmpresaRedSocial redActual = new();

        private void AbrirDialogoNuevaRed()
        {
            redActual = new EmpresaRedSocial
            {
                iIdEmpresa = Data.RegistroSeleccionado.iIdEmpresa,
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
                await Data.PostGetRedesSociales();
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
