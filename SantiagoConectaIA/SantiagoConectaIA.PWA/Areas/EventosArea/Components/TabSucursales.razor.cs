using Microsoft.AspNetCore.Components;

using MudBlazor;

using SantiagoConectaIA.PWA.Areas.EventosArea.Utiles;
using SantiagoConectaIA.Share.Objects.EventosModulo;

using System.Threading.Tasks;

namespace SantiagoConectaIA.PWA.Areas.EventosArea.Components
{
    public partial class TabSucursales : ComponentBase
    {
        [Inject] public MainEventos Data { get; set; }
        [Inject] public ISnackbar Snackbar { get; set; }
        [Parameter] public Evento EventoModel { get; set; } = default!;

        private int editingId = -1;
        private SucursalEvento editModel = new SucursalEvento();

        protected override async Task OnInitializedAsync()
        {
            await base.OnInitializedAsync();
            var targetId = EventoModel?.iIdEvento ?? Data.RegistroSeleccionado.iIdEvento;
            if (targetId > 0)
            {
                await Data.PostGetSucursales(targetId);
            }
        }

        private async Task AgregarSucursal()
        {
            var targetId = EventoModel?.iIdEvento ?? Data.RegistroSeleccionado.iIdEvento;
            if (targetId <= 0)
            {
                Snackbar.Add("Guarda el evento primero antes de agregar sucursales.", Severity.Warning);
                return;
            }

            var nueva = new SucursalEvento
            {
                iIdEvento = targetId,
                vchNombre = "Nueva Sucursal",
                vchDireccion = "",
                vchContacto = "",
                vchTelefono = "",
                vchHorario = "",
                vchNotas = "",
                bActivo = true
            };

            var result = await Data.PostSaveSucursalItem(nueva);
            if (result.bResult)
            {
                await Data.PostGetSucursales(targetId);
                Snackbar.Add("Sucursal agregada exitosamente.", Severity.Success);
                StateHasChanged();
            }
            else
            {
                Snackbar.Add(result.vchMessage, Severity.Error);
            }
        }

        private void IniciarEdicion(SucursalEvento sucursal)
        {
            editingId = sucursal.iIdSucursalEvento;
            editModel = new SucursalEvento
            {
                iIdSucursalEvento = sucursal.iIdSucursalEvento,
                iIdEvento = sucursal.iIdEvento,
                vchNombre = sucursal.vchNombre,
                vchDireccion = sucursal.vchDireccion,
                flLatitud = sucursal.flLatitud,
                flLongitud = sucursal.flLongitud,
                vchContacto = sucursal.vchContacto,
                vchTelefono = sucursal.vchTelefono,
                vchHorario = sucursal.vchHorario,
                vchNotas = sucursal.vchNotas,
                bActivo = sucursal.bActivo
            };
        }

        private void CancelarEdicion()
        {
            editingId = -1;
            editModel = new SucursalEvento();
        }

        private async Task GuardarEdicion()
        {
            var targetId = EventoModel?.iIdEvento ?? Data.RegistroSeleccionado.iIdEvento;
            var result = await Data.PostSaveSucursalItem(editModel);
            if (result.bResult)
            {
                await Data.PostGetSucursales(targetId);
                Snackbar.Add("Sucursal guardada exitosamente.", Severity.Success);
                CancelarEdicion();
                StateHasChanged();
            }
            else
            {
                Snackbar.Add(result.vchMessage, Severity.Error);
            }
        }
    }
}
