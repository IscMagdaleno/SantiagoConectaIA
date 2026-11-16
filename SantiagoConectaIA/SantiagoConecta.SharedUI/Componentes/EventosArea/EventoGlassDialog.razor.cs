using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using SantiagoConecta.SharedUI.Data;
using SantiagoConectaIA.Share.Objects.EventosModulo;
using SantiagoConectaIA.Share.PostClass.EventosModulo;

namespace SantiagoConecta.SharedUI.Componentes.EventosArea;

public partial class EventoGlassDialog : ComponentBase
{
    [Inject]
    private Data_Eventos DataEventos { get; set; } = default!;

    [Parameter] public Evento Evento { get; set; } = default!;
    [Parameter] public EventCallback OnClose { get; set; }

    private static readonly CultureInfo EsMx = new("es-MX");

    private readonly List<string> _imagenesGaleria = new();
    private int _loadedEventoId;
    private int _currentIndex;
    private double _touchStartX;
    private double _touchStartY;

    private List<string> Imagenes
    {
        get
        {
            var list = new List<string>();
            if (!string.IsNullOrWhiteSpace(Evento?.vchImagenPortada))
            {
                list.Add(Evento.vchImagenPortada);
            }
            if (_imagenesGaleria.Any())
            {
                list.AddRange(_imagenesGaleria.Where(u => !string.IsNullOrWhiteSpace(u)));
            }
            return list.Distinct().ToList();
        }
    }

    protected override async Task OnParametersSetAsync()
    {
        if (Evento != null && Evento.iIdEvento != _loadedEventoId)
        {
            _loadedEventoId = Evento.iIdEvento;
            _currentIndex = 0;
            _imagenesGaleria.Clear();

            if (Evento.iIdEvento > 0)
            {
                try
                {
                    var resp = await DataEventos.PostGetImagenesRegistro(new PostGetImagenesRegistro
                    {
                        vchTablaOrigen = "Eventos",
                        iIdRegistro = Evento.iIdEvento
                    });
                    if (resp?.IsSuccess == true && resp.Data != null)
                    {
                        _imagenesGaleria.AddRange(resp.Data
                            .OrderBy(x => x.iOrden)
                            .Select(x => x.vchUrlImagen)
                            .Where(u => !string.IsNullOrWhiteSpace(u)));
                    }
                }
                catch
                {
                    // Ignorar si falla la carga de galería adicional
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
        if (OnClose.HasDelegate)
        {
            await OnClose.InvokeAsync();
        }
    }
}
