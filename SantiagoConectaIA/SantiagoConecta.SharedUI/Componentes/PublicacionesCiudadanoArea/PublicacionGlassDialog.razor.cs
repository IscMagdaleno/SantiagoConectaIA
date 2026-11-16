using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using SantiagoConecta.SharedUI.Data;
using SantiagoConectaIA.Share.Objects.PublicacionCiudadanoModule;
using SantiagoConectaIA.Share.PostClass.EventosModulo;

namespace SantiagoConecta.SharedUI.Componentes.PublicacionesCiudadanoArea
{
    public partial class PublicacionGlassDialog
    {
        [Inject]
        private Data_Eventos DataEventos { get; set; } = default!;

        [Parameter, EditorRequired]
        public PublicacionCiudadano Publicacion { get; set; } = default!;

        [Parameter]
        public string? ImagenUrl { get; set; }

        [Parameter]
        public EventCallback OnClose { get; set; }

        private static readonly CultureInfo EsMx = new("es-MX");

        private readonly List<string> _imagenesExtra = new();
        private int _loadedPublicacionId;
        private int _currentIndex;
        private double _touchStartX;
        private double _touchStartY;

        private List<string> Imagenes
        {
            get
            {
                var list = new List<string>();
                if (Publicacion?.Imagenes != null && Publicacion.Imagenes.Any())
                {
                    list.AddRange(Publicacion.Imagenes
                        .OrderBy(i => i.iOrdenVisualizacion)
                        .Select(i => i.nvchUrlImagen)
                        .Where(u => !string.IsNullOrWhiteSpace(u)));
                }

                if (_imagenesExtra.Any())
                {
                    list.AddRange(_imagenesExtra.Where(u => !string.IsNullOrWhiteSpace(u)));
                }

                if (!string.IsNullOrWhiteSpace(Publicacion?.vchPrimeraImagenUrl))
                {
                    if (!list.Contains(Publicacion.vchPrimeraImagenUrl))
                    {
                        list.Insert(0, Publicacion.vchPrimeraImagenUrl);
                    }
                }

                if (!string.IsNullOrWhiteSpace(ImagenUrl))
                {
                    if (!list.Contains(ImagenUrl))
                    {
                        list.Insert(0, ImagenUrl);
                    }
                }

                return list.Distinct().ToList();
            }
        }

        protected override async Task OnParametersSetAsync()
        {
            if (Publicacion != null && Publicacion.iIdPublicacion != _loadedPublicacionId)
            {
                _loadedPublicacionId = Publicacion.iIdPublicacion;
                _currentIndex = 0;
                _imagenesExtra.Clear();

                if (Publicacion.iIdPublicacion > 0 && (Publicacion.Imagenes == null || Publicacion.Imagenes.Count <= 1))
                {
                    try
                    {
                        var resp = await DataEventos.PostGetImagenesRegistro(new PostGetImagenesRegistro
                        {
                            vchTablaOrigen = "PublicacionCiudadano",
                            iIdRegistro = Publicacion.iIdPublicacion
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

        private static string FormatearTiempo(DateTime fecha)
        {
            var diff = DateTime.UtcNow - fecha;
            if (diff.TotalMinutes < 1) return "Justo ahora";
            if (diff.TotalMinutes < 60) return $"Hace {(int)diff.TotalMinutes} min";
            if (diff.TotalHours < 24) return $"Hace {(int)diff.TotalHours} h";
            if (diff.TotalDays < 7) return $"Hace {(int)diff.TotalDays} d";
            return fecha.ToString("d 'de' MMMM", EsMx);
        }
    }
}
