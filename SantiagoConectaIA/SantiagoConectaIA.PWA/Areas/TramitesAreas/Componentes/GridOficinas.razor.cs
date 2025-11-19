using EngramaCoreStandar.Extensions;

using Microsoft.AspNetCore.Components;

using SantiagoConectaIA.PWA.Areas.TramitesAreas.Utiles;
using SantiagoConectaIA.PWA.Shared.Common;
using SantiagoConectaIA.Share.Objects.OficinasModule;

namespace SantiagoConectaIA.PWA.Areas.TramitesAreas.Componentes
{
	public partial class GridOficinas : EngramaComponent
	{

		[Parameter] public MainTramites Data { get; set; }

		public bool bShowFormOficinas;

		private string filtroTexto = "";
		private List<Oficina> listaFiltrada = new List<Oficina>();


		protected override async Task OnInitializedAsync()
		{

			Loading.Show();
			ShowSnake(await Data.PostGetOficinas());
			ActualizarListaFiltrada();
			Loading.Hide();
		}


		private void OnClickShowOrHideForm()
		{
			Data.OficinaSelected = new();
			bShowFormOficinas = bShowFormOficinas.False();
		}


		private async Task CuandoCambiaTexto(string nuevoTexto)
		{
			filtroTexto = nuevoTexto;
			ActualizarListaFiltrada();
			StateHasChanged();
		}

		private void OnOficinaSaved()
		{
			Data.OficinaSelected = new();
			bShowFormOficinas = false;
		}
		private void OnOficinaSelected(Oficina oficina)
		{
			Data.OficinaSelected = oficina;
			bShowFormOficinas = true;
		}
		private async Task OnDeleteOficina()
		{
			ActualizarListaFiltrada();
			await Task.Delay(1);
			StateHasChanged();
		}

		private void ActualizarListaFiltrada()
		{
			if (string.IsNullOrEmpty(filtroTexto))
			{
				listaFiltrada = Data.LstOficinas.ToList();
				return;
			}
			listaFiltrada = Data.LstOficinas
				.Where(tramite => tramite.vchNombre.ToLower().Contains(filtroTexto.ToLower()))
				.ToList();
		}
	}
}
