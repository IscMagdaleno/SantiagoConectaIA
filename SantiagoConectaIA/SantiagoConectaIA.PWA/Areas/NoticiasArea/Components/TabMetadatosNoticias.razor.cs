using Microsoft.AspNetCore.Components;
using MudBlazor;
using SantiagoConectaIA.PWA.Areas.NoticiasArea.Utiles;
using SantiagoConectaIA.Share.Objects.NoticiasModule;
using System.Collections.Generic;

using System.Threading.Tasks;

namespace SantiagoConectaIA.PWA.Areas.NoticiasArea.Components
{
    public class TabMetadatosNoticiasBase : ComponentBase
    {
        [Inject] public ISnackbar Snackbar { get; set; }

        [Parameter] public MainNoticias Data { get; set; }

        protected NoticiaMetadato nuevoMetadato = new NoticiaMetadato { iIdTipoDato = TipoDatoMetadato.Texto };

        protected override async Task OnInitializedAsync()
        {
            if (Data != null && (Data.LstTipoDatos == null || !Data.LstTipoDatos.Any()))
            {
                await Data.PostGetTipoDatos();
            }
        }

        protected void AgregarMetadato()
        {
            if (string.IsNullOrWhiteSpace(nuevoMetadato.nvchValor))
            {
                Snackbar.Add("El contenido no puede estar vacío", Severity.Warning);
                return;
            }

            if (Data.NoticiaSelected.Metadatos == null)
            {
                Data.NoticiaSelected.Metadatos = new List<NoticiaMetadato>();
            }

            nuevoMetadato.iOrden = Data.NoticiaSelected.Metadatos.Count + 1;
            Data.NoticiaSelected.Metadatos.Add(nuevoMetadato);
            
            // Reset form
            nuevoMetadato = new NoticiaMetadato { iIdTipoDato = TipoDatoMetadato.Texto };
            StateHasChanged();
        }

        protected void EliminarMetadato(NoticiaMetadato metadato)
        {
            Data.NoticiaSelected.Metadatos.Remove(metadato);
            // Reordenar
            for (int i = 0; i < Data.NoticiaSelected.Metadatos.Count; i++)
            {
                Data.NoticiaSelected.Metadatos[i].iOrden = i + 1;
            }
            StateHasChanged();
        }
    }
}
