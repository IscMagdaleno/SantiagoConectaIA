using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using MudBlazor;
using SantiagoConectaIA.PWA.Areas.NoticiasArea.Utiles;
using SantiagoConectaIA.Share.Objects.NoticiasModule;
using SantiagoConectaIA.PWA.Shared.Workspace;

namespace SantiagoConectaIA.PWA.Areas.NoticiasArea.Components
{
    public class TabMetadatosNoticiasBase : ComponentBase
    {
        [Inject] public ISnackbar Snackbar { get; set; } = default!;

        [Parameter] public MainNoticias Data { get; set; } = default!;
        [Parameter] public TipoEstadoControl EstadoControl { get; set; } = default!;

        // Filas del layout
        protected List<NoticiaFila> _filas = new List<NoticiaFila>();

        // Contenedor general para el Drag & Drop de MudBlazor
        protected MudDropContainer<DropItem> _dropContainer;
        protected List<DropItem> _dropItems = new List<DropItem>();

        protected DropItem? itemSeleccionado = null;
        protected RedesSocialesConfig RedesConfig = new RedesSocialesConfig();

        // Form fields
        protected int blockWidth { get; set; } = 12;
        protected string blockFloat { get; set; } = "none";
        protected string blockHeight { get; set; } = "auto";
        protected string blockConfigExtra { get; set; } = string.Empty;

        // Galería
        protected List<string> tempGalleryUrls = new List<string>();
        protected bool _isUploadingGallery = false;

        protected override async Task OnInitializedAsync()
        {
            if (Data != null)
            {
                if (Data.LstTipoDatos == null || !Data.LstTipoDatos.Any())
                {
                    await Data.PostGetTipoDatos();
                }

                if (Data.LstTipoDatos != null)
                {
                    if (!Data.LstTipoDatos.Any(t => t.iIdTipoDato == (int)TipoDatoMetadato.Video))
                        Data.LstTipoDatos.Add(new TipoDatoDto { iIdTipoDato = (int)TipoDatoMetadato.Video, nvchTipo = "Video" });
                    
                    if (!Data.LstTipoDatos.Any(t => t.iIdTipoDato == (int)TipoDatoMetadato.GaleriaImagenes))
                        Data.LstTipoDatos.Add(new TipoDatoDto { iIdTipoDato = (int)TipoDatoMetadato.GaleriaImagenes, nvchTipo = "Galería de Imágenes" });
                    
                    if (!Data.LstTipoDatos.Any(t => t.iIdTipoDato == (int)TipoDatoMetadato.Contenido))
                        Data.LstTipoDatos.Add(new TipoDatoDto { iIdTipoDato = (int)TipoDatoMetadato.Contenido, nvchTipo = "Contenido" });
                }

                LoadFromData();
            }
        }

        protected override void OnParametersSet()
        {
            if (Data?.NoticiaSelected != null)
            {
                LoadFromData();
            }
        }

        private void LoadFromData()
        {
            if (Data.NoticiaSelected == null) return;

            _filas = Data.NoticiaSelected.Filas?.OrderBy(f => f.iOrden).ToList() ?? new List<NoticiaFila>();
            
            _dropItems.Clear();
            
            // 1. Cargar las plantillas en el Sidebar
            _dropItems.Add(new DropItem { Zone = "sidebar", IsTemplate = true, Metadato = new NoticiaMetadato { iIdTipoDato = TipoDatoMetadato.Contenido, vchTitulo = "Bloque de Texto", iAncho = 12 } });
            _dropItems.Add(new DropItem { Zone = "sidebar", IsTemplate = true, Metadato = new NoticiaMetadato { iIdTipoDato = TipoDatoMetadato.Video, vchTitulo = "Video YouTube/MP4", iAncho = 12 } });
            _dropItems.Add(new DropItem { Zone = "sidebar", IsTemplate = true, Metadato = new NoticiaMetadato { iIdTipoDato = TipoDatoMetadato.GaleriaImagenes, vchTitulo = "Galería Fotos", iAncho = 12 } });
            _dropItems.Add(new DropItem { Zone = "sidebar", IsTemplate = true, Metadato = new NoticiaMetadato { iIdTipoDato = TipoDatoMetadato.ImagenSola, vchTitulo = "Imagen Sola", iAncho = 12 } });
            _dropItems.Add(new DropItem { Zone = "sidebar", IsTemplate = true, Metadato = new NoticiaMetadato { iIdTipoDato = TipoDatoMetadato.RedesSociales, vchTitulo = "Redes Sociales", iAncho = 12 } });
            _dropItems.Add(new DropItem { Zone = "sidebar", IsTemplate = true, Metadato = new NoticiaMetadato { iIdTipoDato = TipoDatoMetadato.DatosCuriosos, vchTitulo = "Datos Curiosos", iAncho = 12 } });

            // 2. Cargar los bloques existentes en las filas
            foreach (var fila in _filas)
            {
                if (fila.Metadatos != null)
                {
                    foreach (var meta in fila.Metadatos.OrderBy(m => m.iOrden))
                    {
                        _dropItems.Add(new DropItem
                        {
                            Zone = $"row-{fila.UI_Id}",
                            Metadato = meta,
                            IsTemplate = false,
                            Order = meta.iOrden
                        });
                    }
                }
            }
            
            _dropContainer?.Refresh();
        }

        protected void AddFilaVacia()
        {
            var fila = new NoticiaFila
            {
                iIdNoticia = Data.NoticiaSelected.iIdNoticia,
                iOrden = _filas.Count + 1,
                UI_Id = Guid.NewGuid().ToString()
            };
            _filas.Add(fila);
            SyncData();
            _dropContainer?.Refresh();
        }

        protected void MoveFilaUp(NoticiaFila fila)
        {
            int index = _filas.IndexOf(fila);
            if (index > 0)
            {
                _filas.RemoveAt(index);
                _filas.Insert(index - 1, fila);
                SyncData();
            }
        }

        protected void MoveFilaDown(NoticiaFila fila)
        {
            int index = _filas.IndexOf(fila);
            if (index < _filas.Count - 1 && index >= 0)
            {
                _filas.RemoveAt(index);
                _filas.Insert(index + 1, fila);
                SyncData();
            }
        }

        protected void DeleteFila(NoticiaFila fila)
        {
            _filas.Remove(fila);
            // Eliminar bloques de esta fila
            var toRemove = _dropItems.Where(d => d.Zone == $"row-{fila.UI_Id}").ToList();
            foreach (var rm in toRemove)
            {
                if (itemSeleccionado == rm) CancelarSeleccion();
                _dropItems.Remove(rm);
            }
            SyncData();
            _dropContainer?.Refresh();
        }

        protected void ItemDropped(MudItemDropInfo<DropItem> dropInfo)
        {
            if (dropInfo.Item == null) return;

            var targetZone = dropInfo.DropzoneIdentifier;

            // Si es un template arrastrado desde el sidebar a una fila
            if (dropInfo.Item.IsTemplate && targetZone.StartsWith("row-"))
            {
                // Clonar el metadato
                var nuevoMetadato = new NoticiaMetadato
                {
                    iIdTipoDato = dropInfo.Item.Metadato.iIdTipoDato,
                    vchTitulo = dropInfo.Item.Metadato.vchTitulo,
                    iAncho = 12,
                    vchAlineacion = "none",
                    vchAlto = "auto",
                    nvchValor = dropInfo.Item.Metadato.iIdTipoDato == TipoDatoMetadato.Contenido ? "Contenido de ejemplo..." : "",
                    nvchConfiguracion = "{}"
                };

                // Insertar nuevo item en el dropzone destino
                var newDropItem = new DropItem
                {
                    Zone = targetZone,
                    IsTemplate = false,
                    Metadato = nuevoMetadato
                };

                // Calcular el orden correcto e insertarlo
                var zoneItems = _dropItems.Where(i => i.Zone == targetZone && !i.IsTemplate).OrderBy(i => i.Order).ToList();
                int dropIndex = dropInfo.IndexInZone;
                
                _dropItems.Add(newDropItem);
                
                // Reordenar los elementos de la zona objetivo
                zoneItems.Insert(dropIndex, newDropItem);
                for (int i = 0; i < zoneItems.Count; i++)
                {
                    zoneItems[i].Order = i + 1;
                    zoneItems[i].Metadato.iOrden = i + 1;
                }

                // El item original (template) se mantiene intacto en la lista (no lo movemos)
            }
            else if (!dropInfo.Item.IsTemplate && targetZone.StartsWith("row-"))
            {
                // Moviendo un bloque existente a otra zona o reordenando en la misma
                dropInfo.Item.Zone = targetZone;

                // Reordenar la zona destino
                var zoneItems = _dropItems.Where(i => i.Zone == targetZone && !i.IsTemplate && i != dropInfo.Item).OrderBy(i => i.Order).ToList();
                int dropIndex = dropInfo.IndexInZone;
                if (dropIndex > zoneItems.Count) dropIndex = zoneItems.Count;
                
                zoneItems.Insert(dropIndex, dropInfo.Item);
                
                for (int i = 0; i < zoneItems.Count; i++)
                {
                    zoneItems[i].Order = i + 1;
                    zoneItems[i].Metadato.iOrden = i + 1;
                }
            }

            SyncData();
            _dropContainer?.Refresh();
            StateHasChanged();
        }

        protected void SyncData()
        {
            // Reconstruir la lista Filas con sus Metadatos ordenados
            for (int i = 0; i < _filas.Count; i++)
            {
                _filas[i].iOrden = i + 1;
                var bloquesFila = _dropItems
                    .Where(d => d.Zone == $"row-{_filas[i].UI_Id}" && !d.IsTemplate)
                    .OrderBy(d => d.Order)
                    .Select(d => d.Metadato)
                    .ToList();
                
                foreach(var b in bloquesFila)
                {
                    b.iIdNoticia = Data.NoticiaSelected.iIdNoticia;
                    if (_filas[i].iIdFila > 0)
                        b.iIdFila = _filas[i].iIdFila;
                }
                
                _filas[i].Metadatos = bloquesFila;
            }

            Data.NoticiaSelected.Filas = _filas;
        }

        protected void SeleccionarBloque(DropItem item)
        {
            if (item.IsTemplate) return;
            
            itemSeleccionado = item;
            var meta = item.Metadato;

            blockWidth = meta.iAncho ?? 12;
            blockFloat = string.IsNullOrEmpty(meta.vchAlineacion) ? "none" : meta.vchAlineacion;
            blockHeight = string.IsNullOrEmpty(meta.vchAlto) ? "auto" : meta.vchAlto;
            blockConfigExtra = string.IsNullOrEmpty(meta.nvchConfiguracion) ? "{}" : meta.nvchConfiguracion;

            tempGalleryUrls.Clear();
            if (meta.iIdTipoDato == TipoDatoMetadato.GaleriaImagenes && !string.IsNullOrEmpty(meta.nvchValor))
            {
                var urls = meta.nvchValor.Split(';', StringSplitOptions.RemoveEmptyEntries);
                tempGalleryUrls.AddRange(urls);
            }

            if (meta.iIdTipoDato == TipoDatoMetadato.RedesSociales)
            {
                if (!string.IsNullOrEmpty(meta.nvchConfiguracion))
                {
                    try { RedesConfig = System.Text.Json.JsonSerializer.Deserialize<RedesSocialesConfig>(meta.nvchConfiguracion) ?? new RedesSocialesConfig(); }
                    catch { RedesConfig = new RedesSocialesConfig(); }
                }
                else { RedesConfig = new RedesSocialesConfig(); }
            }

            StateHasChanged();
        }

        protected void OnRedesSocialesChanged()
        {
            if (itemSeleccionado != null && itemSeleccionado.Metadato.iIdTipoDato == TipoDatoMetadato.RedesSociales)
            {
                itemSeleccionado.Metadato.nvchConfiguracion = System.Text.Json.JsonSerializer.Serialize(RedesConfig);
                SyncData();
            }
        }

        protected void CancelarSeleccion()
        {
            itemSeleccionado = null;
            tempGalleryUrls.Clear();
            StateHasChanged();
        }

        protected void GuardarConfiguracionBloque()
        {
            if (itemSeleccionado == null) return;

            var meta = itemSeleccionado.Metadato;

            if (meta.iIdTipoDato == TipoDatoMetadato.GaleriaImagenes)
            {
                meta.nvchValor = string.Join(";", tempGalleryUrls);
            }

            meta.iAncho = blockWidth;
            meta.vchAlineacion = blockFloat;
            meta.vchAlto = blockHeight;
            
            if (meta.iIdTipoDato == TipoDatoMetadato.RedesSociales)
            {
                meta.nvchConfiguracion = System.Text.Json.JsonSerializer.Serialize(RedesConfig);
                blockConfigExtra = meta.nvchConfiguracion; // Sincroniza la caja de texto extra por si acaso
            }
            else
            {
                meta.nvchConfiguracion = blockConfigExtra;
            }

            Snackbar.Add("Bloque actualizado correctamente", Severity.Success);
            SyncData();
            StateHasChanged();
        }

        protected void EliminarBloqueSeleccionado()
        {
            if (itemSeleccionado != null)
            {
                _dropItems.Remove(itemSeleccionado);
                CancelarSeleccion();
                SyncData();
                Snackbar.Add("Bloque eliminado", Severity.Info);
            }
        }

        protected void UpdateBlockWidth(int width)
        {
            blockWidth = width;
            if (itemSeleccionado != null)
            {
                itemSeleccionado.Metadato.iAncho = width;
                SyncData();
            }
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
                    if (itemSeleccionado != null && itemSeleccionado.Metadato.iIdTipoDato == TipoDatoMetadato.GaleriaImagenes)
                    {
                         itemSeleccionado.Metadato.nvchValor = string.Join(";", tempGalleryUrls);
                         SyncData();
                    }
                    Snackbar.Add("Imagen agregada a la galería", Severity.Success);
                }
                else
                {
                    Snackbar.Add("Error al subir el archivo", Severity.Error);
                }
            }
            catch (Exception ex)
            {
                Snackbar.Add($"Error: {ex.Message}", Severity.Error);
            }
            finally
            {
                _isUploadingGallery = false;
                StateHasChanged();
            }
        }

        protected async Task OnUploadSingleImage(IBrowserFile file)
        {
            if (file == null) return;
            _isUploadingGallery = true;
            StateHasChanged();
            try
            {
                var url = await Data.UploadGenericFile(file);
                if (!string.IsNullOrEmpty(url))
                {
                    if (itemSeleccionado != null && itemSeleccionado.Metadato.iIdTipoDato == TipoDatoMetadato.ImagenSola)
                    {
                         itemSeleccionado.Metadato.nvchValor = url;
                         SyncData();
                    }
                    Snackbar.Add("Imagen subida correctamente", Severity.Success);
                }
            }
            catch (Exception ex) { Snackbar.Add($"Error: {ex.Message}", Severity.Error); }
            finally { _isUploadingGallery = false; StateHasChanged(); }
        }

        protected void RemoveGalleryImage(string url)
        {
            tempGalleryUrls.Remove(url);
            if (itemSeleccionado != null && itemSeleccionado.Metadato.iIdTipoDato == TipoDatoMetadato.GaleriaImagenes)
            {
                itemSeleccionado.Metadato.nvchValor = string.Join(";", tempGalleryUrls);
                SyncData();
            }
            StateHasChanged();
        }

        protected string GetBlockIcon(TipoDatoMetadato tipoDato)
        {
            return tipoDato switch
            {
                TipoDatoMetadato.Video => Icons.Material.Filled.PlayCircle,
                TipoDatoMetadato.GaleriaImagenes => Icons.Material.Filled.PhotoLibrary,
                TipoDatoMetadato.Contenido => Icons.Material.Filled.Article,
                TipoDatoMetadato.ImagenSola => Icons.Material.Filled.Image,
                TipoDatoMetadato.RedesSociales => Icons.Material.Filled.Share,
                TipoDatoMetadato.DatosCuriosos => Icons.Material.Filled.Lightbulb,
                _ => Icons.Material.Filled.Category
            };
        }

        protected Color GetBlockIconColor(TipoDatoMetadato tipoDato)
        {
            return tipoDato switch
            {
                TipoDatoMetadato.Video => Color.Error,
                TipoDatoMetadato.GaleriaImagenes => Color.Success,
                TipoDatoMetadato.Contenido => Color.Info,
                TipoDatoMetadato.ImagenSola => Color.Info,
                TipoDatoMetadato.RedesSociales => Color.Secondary,
                TipoDatoMetadato.DatosCuriosos => Color.Warning,
                _ => Color.Default
            };
        }
        
        protected string GetWidthPercent(int? cols) 
        {
            return cols switch {
                3 => "25%",
                6 => "50%",
                9 => "75%",
                _ => "100%"
            };
        }
    }
	public class DropItem
	{
		public string Id { get; set; } = Guid.NewGuid().ToString();
		public string Zone { get; set; } = string.Empty;
		public NoticiaMetadato Metadato { get; set; } = new NoticiaMetadato();
		public bool IsTemplate { get; set; } = false;
		public int Order { get; set; } = 0;
	}

	public class RedesSocialesConfig
	{
		public string Facebook { get; set; } = string.Empty;
		public string WhatsApp { get; set; } = string.Empty;
		public string Instagram { get; set; } = string.Empty;
		public string TikTok { get; set; } = string.Empty;
		public string X { get; set; } = string.Empty;
	}
}
