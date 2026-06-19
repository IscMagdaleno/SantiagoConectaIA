using Microsoft.AspNetCore.Components;
using SantiagoConectaIA.PWA.Areas.EmpresasArea.Utiles;
using SantiagoConectaIA.Share.Objetos.EmpresasModulo;
using System.Threading.Tasks;
using MudBlazor;

namespace SantiagoConectaIA.PWA.Areas.EmpresasArea.Components
{
    public partial class TabUbicaciones : ComponentBase
    {
        [Inject] public MainEmpresas Data { get; set; }
        [Inject] public ISnackbar Snackbar { get; set; }

        private bool dialogVisible = false;
        private DialogOptions dialogOptions = new() { MaxWidth = MaxWidth.Medium, FullWidth = true, CloseButton = true };
        private EmpresaUbicacion ubicacionActual = new();

        private void AbrirDialogoNuevaUbicacion()
        {
            ubicacionActual = new EmpresaUbicacion
            {
                iIdEmpresa = Data.RegistroSeleccionado.iIdEmpresa,
                bActivo = true
            };
            dialogVisible = true;
        }

        private void EditarUbicacion(EmpresaUbicacion ubicacion)
        {
            ubicacionActual = new EmpresaUbicacion
            {
                iIdUbicacion = ubicacion.iIdUbicacion,
                iIdEmpresa = ubicacion.iIdEmpresa,
                vchAlias = ubicacion.vchAlias,
                vchDireccion = ubicacion.vchDireccion,
                flLatitud = ubicacion.flLatitud,
                flLongitud = ubicacion.flLongitud,
                bActivo = ubicacion.bActivo
            };
            dialogVisible = true;
        }

        private void CerrarDialogo()
        {
            dialogVisible = false;
        }

        private async Task GuardarUbicacion()
        {
            if (string.IsNullOrWhiteSpace(ubicacionActual.vchAlias) || string.IsNullOrWhiteSpace(ubicacionActual.vchDireccion))
            {
                Snackbar.Add("El alias y la dirección son requeridos.", Severity.Warning);
                return;
            }

            var result = await Data.PostSaveUbicacion(ubicacionActual);
            if (result.bResult)
            {
                await Data.PostGetUbicaciones();
                Snackbar.Add("Ubicación guardada exitosamente.", Severity.Success);
                dialogVisible = false;
            }
            else
            {
                Snackbar.Add(result.vchMessage, Severity.Error);
            }
        }
    }
}
