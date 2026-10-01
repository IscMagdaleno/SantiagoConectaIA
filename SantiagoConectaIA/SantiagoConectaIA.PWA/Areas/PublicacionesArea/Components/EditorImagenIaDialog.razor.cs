using Microsoft.AspNetCore.Components;
using MudBlazor;
using SantiagoConectaIA.PWA.Areas.PublicacionesArea.Utiles;
using SantiagoConectaIA.Share.Utilities;

namespace SantiagoConectaIA.PWA.Areas.PublicacionesArea.Components
{
    public partial class EditorImagenIaDialog : ComponentBase
    {
        [CascadingParameter] public IMudDialogInstance MudDialog { get; set; } = default!;
        [Inject] public MainPublicaciones Publicaciones { get; set; } = default!;
        [Inject] public ISnackbar Snackbar { get; set; } = default!;

        [Parameter] public string ImagenUrl { get; set; } = string.Empty;

        public IReadOnlyList<PromptImagen> Prompts => PromptsImagen.Lista;
        public PromptImagen? PromptSeleccionado { get; set; }
        public bool PromptPropio { get; set; }
        public string Prompt { get; set; } = string.Empty;
        public string? ImagenEditada { get; set; }
        public bool Editando { get; set; }

        public bool PuedeEditar => !Editando && !string.IsNullOrWhiteSpace(Prompt);

        public static async Task<string?> AbrirAsync(IDialogService dialogService, string imagenUrl)
        {
            var parameters = new DialogParameters<EditorImagenIaDialog> { { d => d.ImagenUrl, imagenUrl } };
            var options = new DialogOptions
            {
                MaxWidth = MaxWidth.Large,
                FullWidth = true,
                CloseButton = false,
                BackdropClick = false
            };

            var dialog = await dialogService.ShowAsync<EditorImagenIaDialog>("Editar imagen con IA", parameters, options);
            var result = await dialog.Result;
            return result is { Canceled: false, Data: string url } && !string.IsNullOrWhiteSpace(url) ? url : null;
        }

        public void ElegirPrompt(PromptImagen prompt)
        {
            PromptSeleccionado = prompt;
            PromptPropio = false;
            Prompt = prompt.Prompt;
        }

        public void ElegirPromptPropio()
        {
            PromptSeleccionado = null;
            PromptPropio = true;
            Prompt = string.Empty;
        }

        public async Task Editar()
        {
            if (!PuedeEditar)
            {
                return;
            }

            Editando = true;
            var result = await Publicaciones.EditarImagenConIa(ImagenUrl, Prompt.Trim());
            Editando = false;

            if (result.IsSuccess && !string.IsNullOrWhiteSpace(result.Data))
            {
                ImagenEditada = result.Data;
                Snackbar.Add("Imagen editada. Compara el antes y el después.", Severity.Success);
            }
            else
            {
                Snackbar.Add(string.IsNullOrWhiteSpace(result.Message) ? "No se pudo editar la imagen." : result.Message, Severity.Error);
            }
        }

        public static string Extracto(string texto)
        {
            return texto.Length > 110 ? $"{texto[..110].TrimEnd()}…" : texto;
        }

        public void Usar() => MudDialog.Close(DialogResult.Ok(ImagenEditada));

        public void Cancelar() => MudDialog.Cancel();
    }
}
