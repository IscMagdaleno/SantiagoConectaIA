using Microsoft.AspNetCore.Components;
using MudBlazor;
using SantiagoConectaIA.PWA.Areas.OficinasArea.Utiles;
using SantiagoConectaIA.PWA.Shared.Common;
using SantiagoConectaIA.Share.Objects.OficinasModule;
using System.Threading.Tasks;

namespace SantiagoConectaIA.PWA.Areas.OficinasArea.Components
{
    public partial class CardOficina : EngramaComponent
    {
        [Inject] public MainOficinas Data { get; set; }

        private async Task OnClickVer()
        {
            await OnView.InvokeAsync(Oficina);
        }

        private async Task OnClickEditar()
        {
            await OnEdit.InvokeAsync(Oficina);
        }

        private async Task OnClickEliminar(Oficina oficina)
        {
            var parameters = new DialogParameters { { "ContentText", "¿Estás seguro de eliminar esta oficina?" } };
            var dialog = await DialogService.ShowAsync<ConfirmationDialog>("Confirmar eliminación", parameters);
            var result = await dialog.Result;

            if (!result.Canceled)
            {
                Loading.Show();
                oficina.bActivo = false;
                Data.OficinaSelected = oficina;
                var saveResult = await Data.PostSaveOficina();
                ShowSnake(saveResult);
                if (saveResult.bResult)
                {
                    await OnDelete.InvokeAsync(oficina);
                }
                Loading.Hide();
            }
        }
    }
}
