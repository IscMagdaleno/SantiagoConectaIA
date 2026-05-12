using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using MudBlazor;
using SantiagoConectaIA.PWA.Areas.NoticiasArea.Utiles;
using SantiagoConectaIA.Share.Objects.NoticiasModule;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SantiagoConectaIA.PWA.Areas.NoticiasArea.Components
{
    public class TabMetadatosNoticiasBase : ComponentBase
    {
        [Inject] public ISnackbar Snackbar { get; set; }

        [Parameter] public MainNoticias Data { get; set; }

        protected NoticiaMetadato nuevoMetadato = new NoticiaMetadato { iIdTipoDato = TipoDatoMetadato.Video };

        // Almacenamiento temporal para URLs de imágenes de la galería
        protected List<string> tempGalleryUrls = new List<string>();
        protected bool _isUploadingGallery = false;

        // Metadato seleccionado para edición
        protected NoticiaMetadato? metadatoSeleccionado = null;

        protected override async Task OnInitializedAsync()
        {
            if (Data != null && (Data.LstTipoDatos == null || !Data.LstTipoDatos.Any()))
            {
                await Data.PostGetTipoDatos();
            }
        }

        protected void SeleccionarMetadato(NoticiaMetadato metadato)
        {
            metadatoSeleccionado = metadato;
            
            // Copiar valores al objeto de edición para no mutar el original en vivo antes de guardar
            nuevoMetadato = new NoticiaMetadato
            {
                iIdMetadato = metadato.iIdMetadato,
                iIdNoticia = metadato.iIdNoticia,
                iIdTipoDato = metadato.iIdTipoDato,
                vchAlias = metadato.vchAlias,
                nvchValor = metadato.nvchValor,
                iOrden = metadato.iOrden
            };

            tempGalleryUrls.Clear();
            if (metadato.iIdTipoDato == TipoDatoMetadato.GaleriaImagenes && !string.IsNullOrEmpty(metadato.nvchValor))
            {
                var urls = metadato.nvchValor.Split(';', System.StringSplitOptions.RemoveEmptyEntries);
                tempGalleryUrls.AddRange(urls);
            }

            StateHasChanged();
        }

        protected void CancelarSeleccion()
        {
            metadatoSeleccionado = null;
            nuevoMetadato = new NoticiaMetadato { iIdTipoDato = TipoDatoMetadato.Video };
            tempGalleryUrls.Clear();
            StateHasChanged();
        }

        protected void AgregarMetadato()
        {
            if (nuevoMetadato.iIdTipoDato == TipoDatoMetadato.GaleriaImagenes)
            {
                if (tempGalleryUrls.Count < 3)
                {
                    Snackbar.Add("Se requiere subir al menos 3 imágenes para la galería", Severity.Warning);
                    return;
                }
                // Serializar lista de imágenes separadas por punto y coma
                nuevoMetadato.nvchValor = string.Join(";", tempGalleryUrls);
            }
            else if (string.IsNullOrWhiteSpace(nuevoMetadato.nvchValor))
            {
                Snackbar.Add("El contenido no puede estar vacío", Severity.Warning);
                return;
            }

            if (metadatoSeleccionado == null)
            {
                // Modo: Agregar nuevo bloque
                if (Data.NoticiaSelected.Metadatos == null)
                {
                    Data.NoticiaSelected.Metadatos = new List<NoticiaMetadato>();
                }

                nuevoMetadato.iOrden = Data.NoticiaSelected.Metadatos.Count + 1;
                Data.NoticiaSelected.Metadatos.Add(nuevoMetadato);
                Snackbar.Add("Bloque de metadato agregado", Severity.Success);
            }
            else
            {
                // Modo: Editar bloque existente
                var original = Data.NoticiaSelected.Metadatos.FirstOrDefault(m => m.iOrden == metadatoSeleccionado.iOrden);
                if (original != null)
                {
                    original.iIdTipoDato = nuevoMetadato.iIdTipoDato;
                    original.vchAlias = nuevoMetadato.vchAlias;
                    original.nvchValor = nuevoMetadato.nvchValor;
                    Snackbar.Add("Bloque de metadato actualizado", Severity.Success);
                }
            }
            
            // Forzar actualización de referencia para refrescar la UI de Blazor
            Data.NoticiaSelected.Metadatos = new List<NoticiaMetadato>(Data.NoticiaSelected.Metadatos);
            
            // Reset form
            metadatoSeleccionado = null;
            nuevoMetadato = new NoticiaMetadato { iIdTipoDato = TipoDatoMetadato.Video };
            tempGalleryUrls.Clear();
            StateHasChanged();
        }

        protected void EliminarMetadato(NoticiaMetadato metadato)
        {
            // Si el bloque a eliminar estaba seleccionado, cancelar selección primero
            if (metadatoSeleccionado != null && metadatoSeleccionado.iOrden == metadato.iOrden)
            {
                CancelarSeleccion();
            }

            Data.NoticiaSelected.Metadatos.Remove(metadato);
            // Reordenar
            for (int i = 0; i < Data.NoticiaSelected.Metadatos.Count; i++)
            {
                Data.NoticiaSelected.Metadatos[i].iOrden = i + 1;
            }
            
            // Forzar actualización de referencia
            Data.NoticiaSelected.Metadatos = new List<NoticiaMetadato>(Data.NoticiaSelected.Metadatos);
            
            StateHasChanged();
        }

        protected void MoverArriba(NoticiaMetadato metadato)
        {
            if (metadato == null || Data.NoticiaSelected.Metadatos == null) return;
            
            var metadatosOrdenados = Data.NoticiaSelected.Metadatos.OrderBy(m => m.iOrden).ToList();
            int index = metadatosOrdenados.IndexOf(metadato);
            if (index <= 0) return;
            
            var anterior = metadatosOrdenados[index - 1];
            metadatosOrdenados[index - 1] = metadato;
            metadatosOrdenados[index] = anterior;
            
            for (int i = 0; i < metadatosOrdenados.Count; i++)
            {
                metadatosOrdenados[i].iOrden = i + 1;
            }
            
            Data.NoticiaSelected.Metadatos = metadatosOrdenados;
            StateHasChanged();
        }

        protected void MoverAbajo(NoticiaMetadato metadato)
        {
            if (metadato == null || Data.NoticiaSelected.Metadatos == null) return;
            
            var metadatosOrdenados = Data.NoticiaSelected.Metadatos.OrderBy(m => m.iOrden).ToList();
            int index = metadatosOrdenados.IndexOf(metadato);
            if (index < 0 || index >= metadatosOrdenados.Count - 1) return;
            
            var siguiente = metadatosOrdenados[index + 1];
            metadatosOrdenados[index + 1] = metadato;
            metadatosOrdenados[index] = siguiente;
            
            for (int i = 0; i < metadatosOrdenados.Count; i++)
            {
                metadatosOrdenados[i].iOrden = i + 1;
            }
            
            Data.NoticiaSelected.Metadatos = metadatosOrdenados;
            StateHasChanged();
        }

        protected Color GetChipColor(TipoDatoMetadato type)
        {
            return type switch
            {
                TipoDatoMetadato.Video => Color.Primary,
                TipoDatoMetadato.GaleriaImagenes => Color.Success,
                _ => Color.Default
            };
        }

        protected string GetTypeName(TipoDatoMetadato type)
        {
            return type switch
            {
                TipoDatoMetadato.Video => "Video",
                TipoDatoMetadato.GaleriaImagenes => "Galería de Imágenes",
                _ => "Otro"
            };
        }

        protected async Task OnUploadGalleryImage(IBrowserFile file)
        {
            if (file == null) return;

            _isUploadingGallery = true;
            StateHasChanged();

            try
            {
                var url = await Data.UploadGenericFile(file);
                if (!string.IsNullOrEmpty(url))
                {
                    tempGalleryUrls.Add(url);
                    Snackbar.Add("Imagen agregada a la galería temporal", Severity.Success);
                }
                else
                {
                    Snackbar.Add("Error al subir el archivo", Severity.Error);
                }
            }
            catch (System.Exception ex)
            {
                Snackbar.Add($"Error: {ex.Message}", Severity.Error);
            }
            finally
            {
                _isUploadingGallery = false;
                StateHasChanged();
            }
        }

        protected void RemoveGalleryImage(string url)
        {
            tempGalleryUrls.Remove(url);
            Snackbar.Add("Imagen removida", Severity.Info);
            StateHasChanged();
        }
    }
}
