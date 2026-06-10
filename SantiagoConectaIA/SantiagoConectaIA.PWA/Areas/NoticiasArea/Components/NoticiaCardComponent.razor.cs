using Microsoft.AspNetCore.Components;
using SantiagoConectaIA.PWA.Shared.Common;
using MudBlazor;
using SantiagoConectaIA.PWA.Areas.NoticiasArea.Utiles;
using SantiagoConectaIA.Share.Objects.NoticiasModule;

namespace SantiagoConectaIA.PWA.Areas.NoticiasArea.Components
{
    public partial class NoticiaCardComponent : EngramaComponent
    {
        [Inject] public MainNoticias Data { get; set; }
        [Inject] public IDialogService DialogService { get; set; }

        private async Task OnClickVer()
        {
            await OnView.InvokeAsync(Noticia);
        }

        private async Task OnClickEditar()
        {
            await OnEdit.InvokeAsync(Noticia);
        }

        private async Task OnClickEliminarNoticia(Noticia noticia)
        {
            var parameters = new DialogParameters { { "ContentText", "¿Estás seguro de eliminar esta noticia?" } };
            var dialog = await DialogService.ShowAsync<ConfirmationDialog>("Confirmar eliminación", parameters);
            var result = await dialog.Result;

            if (!result.Canceled)
            {
                Loading.Show();
                Data.NoticiaSelected = noticia;
                Data.NoticiaSelected.bActivo = false;

                var saveResult = await Data.PostSaveNoticia();
                ShowSnake(saveResult);
                
                if (saveResult.bResult)
                {
                    await OnDelete.InvokeAsync(noticia);
                }
                Loading.Hide();
            }
        }
    }
}
