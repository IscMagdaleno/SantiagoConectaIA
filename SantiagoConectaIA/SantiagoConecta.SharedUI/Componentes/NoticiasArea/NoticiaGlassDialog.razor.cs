using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using SantiagoConecta.SharedUI.Data;
using SantiagoConectaIA.Share.Objects.NoticiasModule;
using SantiagoConectaIA.Share.PostClass.EventosModulo;

namespace SantiagoConecta.SharedUI.Componentes.NoticiasArea
{
    public partial class NoticiaGlassDialog
    {
        [Inject]
        private Data_Eventos DataEventos { get; set; } = default!;

        [Parameter, EditorRequired]
        public Noticia Noticia { get; set; } = default!;

        [Parameter]
        public EventCallback OnClose { get; set; }

        private static readonly CultureInfo EsMx = new("es-MX");

        private readonly List<string> _imagenesExtra = new();
        private int _loadedNoticiaId;
        private int _currentIndex;
        private double _touchStartX;
        private double _touchStartY;

        private List<string> Imagenes
        {
            get
            {
                var list = new List<string>();
                if (!string.IsNullOrWhiteSpace(Noticia?.vchImagenPortada))
                {
                    list.Add(Noticia.vchImagenPortada);
                }

                if (Noticia?.Imagenes?.Any() == true)
                {
                    list.AddRange(Noticia.Imagenes
                        .Select(x => x.vchUrlImagen)
                        .Where(u => !string.IsNullOrWhiteSpace(u)));
                }

                if (Noticia?.Filas?.Any() == true)
                {
                    foreach (var fila in Noticia.Filas.OrderBy(f => f.iOrden))
                    {
                        if (fila.Metadatos == null) continue;
                        foreach (var meta in fila.Metadatos.OrderBy(m => m.iOrden))
                        {
                            if (meta.iIdTipoDato == TipoDatoMetadato.ImagenSola && !string.IsNullOrWhiteSpace(meta.nvchValor))
                            {
                                list.Add(meta.nvchValor.Trim());
                            }
                            else if (meta.iIdTipoDato == TipoDatoMetadato.GaleriaImagenes && !string.IsNullOrWhiteSpace(meta.nvchValor))
                            {
                                var urls = meta.nvchValor.Split(';', StringSplitOptions.RemoveEmptyEntries);
                                list.AddRange(urls.Select(u => u.Trim()).Where(u => !string.IsNullOrWhiteSpace(u)));
                            }
                        }
                    }
                }

                if (_imagenesExtra.Any())
                {
                    list.AddRange(_imagenesExtra.Where(u => !string.IsNullOrWhiteSpace(u)));
                }

                return list.Distinct().ToList();
            }
        }

        private string ContenidoHtml
        {
            get
            {
                if (!string.IsNullOrWhiteSpace(Noticia?.nvchContenido))
                {
                    return Noticia.nvchContenido;
                }

                if (Noticia?.Filas?.Any() == true)
                {
                    var bloquesTexto = Noticia.Filas
                        .OrderBy(f => f.iOrden)
                        .Where(f => f.Metadatos != null)
                        .SelectMany(f => f.Metadatos.OrderBy(m => m.iOrden))
                        .Where(m => m.iIdTipoDato == TipoDatoMetadato.Contenido && !string.IsNullOrWhiteSpace(m.nvchValor))
                        .Select(m => m.nvchValor);

                    return string.Join("<br/><br/>", bloquesTexto);
                }

                return string.Empty;
            }
        }

        protected override async Task OnParametersSetAsync()
        {
            if (Noticia != null && Noticia.iIdNoticia != _loadedNoticiaId)
            {
                _loadedNoticiaId = Noticia.iIdNoticia;
                _currentIndex = 0;
                _imagenesExtra.Clear();

                if (Noticia.iIdNoticia > 0)
                {
                    try
                    {
                        var resp = await DataEventos.PostGetImagenesRegistro(new PostGetImagenesRegistro
                        {
                            vchTablaOrigen = "Noticias",
                            iIdRegistro = Noticia.iIdNoticia
                        });
                        if (resp?.IsSuccess == true && resp.Data != null)
                        {
                            _imagenesExtra.AddRange(resp.Data
                                .OrderBy(x => x.iOrden)
                                .Select(x => x.vchUrlImagen)
                                .Where(u => !string.IsNullOrWhiteSpace(u)));
                        }
                    }
                    catch
                    {
                        // Ignorar fallo de galería adicional
                    }
                }
            }
        }

        private void PrevSlide()
        {
            if (_currentIndex > 0)
            {
                _currentIndex--;
            }
        }

        private void NextSlide()
        {
            var total = Imagenes.Count;
            if (_currentIndex < total - 1)
            {
                _currentIndex++;
            }
        }

        private void GoToSlide(int index)
        {
            var total = Imagenes.Count;
            if (index >= 0 && index < total)
            {
                _currentIndex = index;
            }
        }

        private void OnTouchStart(TouchEventArgs e)
        {
            if (e.Touches != null && e.Touches.Length > 0)
            {
                _touchStartX = e.Touches[0].ClientX;
                _touchStartY = e.Touches[0].ClientY;
            }
        }

        private void OnTouchEnd(TouchEventArgs e)
        {
            if (e.ChangedTouches != null && e.ChangedTouches.Length > 0)
            {
                var deltaX = e.ChangedTouches[0].ClientX - _touchStartX;
                var deltaY = e.ChangedTouches[0].ClientY - _touchStartY;

                if (Math.Abs(deltaX) > 38 && Math.Abs(deltaX) > Math.Abs(deltaY))
                {
                    if (deltaX < 0)
                    {
                        NextSlide();
                    }
                    else
                    {
                        PrevSlide();
                    }
                }
            }
        }

        private async Task Cerrar()
        {
            await OnClose.InvokeAsync();
        }
    }
}
