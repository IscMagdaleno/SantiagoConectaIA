using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using MudBlazor;
using SantiagoConectaIA.PWA.Areas.EventosArea.Utiles;
using SantiagoConectaIA.Share.Objetos.EventosModulo;
using SantiagoConectaIA.Share.PostClass.EventosModulo;
using System.Threading.Tasks;

namespace SantiagoConectaIA.PWA.Areas.EventosArea.Components
{
    public partial class TabImagenesEvento : ComponentBase
    {
        [Inject] public MainEventos Data { get; set; }
        [Inject] public ISnackbar Snackbar { get; set; }

        private async Task UploadNuevaImagen(IBrowserFile file)
        {
            if (file == null) return;

            var uploadResult = await Data.PostUploadImagen(file);
            if (uploadResult != null && uploadResult.IsSuccess && uploadResult.Data != null && !string.IsNullOrEmpty(uploadResult.Data.URL))
            {
                var nuevaImagen = new PostSaveImagenRegistro
                {
                    vchTablaOrigen = "Eventos",
                    iIdRegistro = Data.RegistroSeleccionado.iIdEvento,
                    vchUrlImagen = uploadResult.Data.URL,
                    vchDescripcion = "",
                    iOrden = Data.LstImagenes?.Count ?? 0
                };

                var result = await Data.PostSaveImagen(nuevaImagen);
                if (result.bResult)
                {
                    await Data.PostGetDetalle(Data.RegistroSeleccionado.iIdEvento);
                    Snackbar.Add("Imagen agregada exitosamente.", Severity.Success);
                    StateHasChanged();
                }
                else
                {
                    Snackbar.Add(result.vchMessage, Severity.Error);
                }
            }
            else
            {
                Snackbar.Add(uploadResult?.Message ?? "Error al subir la imagen.", Severity.Error);
            }
        }

        private async Task EliminarImagen(ImagenRegistro imagen)
        {
            var result = await Data.PostDeleteImagen(imagen.iIdImagen);
            if (result.bResult)
            {
                await Data.PostGetDetalle(Data.RegistroSeleccionado.iIdEvento);
                Snackbar.Add("Imagen eliminada exitosamente.", Severity.Success);
                StateHasChanged();
            }
            else
            {
                Snackbar.Add(result.vchMessage, Severity.Error);
            }
        }
    }
}
