using Microsoft.AspNetCore.Components;
using SantiagoConecta.SharedUI.Data;
using SantiagoConectaIA.Share.Objetos.EventosModulo;
using SantiagoConectaIA.Share.PostClass.EventosModulo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SantiagoConecta.SharedUI.Pages
{
    public partial class EventosDetalle : ComponentBase
    {
        [Inject]
        public NavigationManager NavManager { get; set; } = default!;

        [Inject]
        public Data_Eventos dataEventos { get; set; } = default!;

        [Parameter]
        public int Id { get; set; }

        protected EventoDetalle? selectedEvento;
        protected List<SucursalEvento> sucursalesEvento = new();
        protected List<ImagenRegistro> galeriaImagenes = new();

        protected override async Task OnInitializedAsync()
        {
            try
            {
                var evResp = await dataEventos.PostGetEventoDetalle(Id);
                if (evResp != null && evResp.IsSuccess && evResp.Data != null)
                {
                    selectedEvento = evResp.Data;
                }

                var sucResp = await dataEventos.PostGetEventosSucursales(Id);
                if (sucResp != null && sucResp.IsSuccess && sucResp.Data != null)
                {
                    sucursalesEvento = sucResp.Data.ToList();
                }

                var imgResp = await dataEventos.PostGetImagenesRegistro(new PostGetImagenesRegistro
                {
                    vchTablaOrigen = "Eventos",
                    iIdRegistro = Id
                });
                if (imgResp != null && imgResp.IsSuccess && imgResp.Data != null)
                {
                    galeriaImagenes = imgResp.Data.Where(x => !string.IsNullOrEmpty(x.vchUrlImagen)).ToList();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error cargando detalle del evento: {ex.Message}");
            }
        }

        protected void VolverAlListado()
        {
            NavManager.NavigateTo("/eventos");
        }
    }
}
