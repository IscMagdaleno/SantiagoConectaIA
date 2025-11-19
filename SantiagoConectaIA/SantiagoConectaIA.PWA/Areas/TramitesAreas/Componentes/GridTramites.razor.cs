using Microsoft.AspNetCore.Components;

using SantiagoConectaIA.PWA.Areas.TramitesAreas.Utiles;
using SantiagoConectaIA.PWA.Shared.Common;
using SantiagoConectaIA.Share.Objects.TramitesModule;

namespace SantiagoConectaIA.PWA.Areas.TramitesAreas.Componentes
{
	public partial class GridTramites : EngramaComponent
	{

		[Parameter] public MainTramites Data { get; set; }


		public bool bShowFormTramite;

		private string filtroTexto = "";
		private List<Tramite> listaFiltrada = new List<Tramite>();

		#region CICLO VIDA BLAZOR
		protected override void OnInitialized()
		{

		}
		protected override async Task OnInitializedAsync()
		{

			Loading.Show();
			ShowSnake(await Data.PostGetTramites());
			ActualizarListaFiltrada();
			Loading.Hide();
		}
		#endregion


		private void OnTramiteSelected(Tramite tramite)
		{
			Data.TramiteSelected = tramite;
			bShowFormTramite = true;
		}

		private void OnClickShowForm()
		{
			Data.TramiteSelected = new();
			bShowFormTramite = true;
		}
		private void OnClickShowData()
		{
			Data.TramiteSelected = new();
			bShowFormTramite = false;
		}

		private void OnTramiteSaved()
		{
			bShowFormTramite = false;

		}

		private async Task OnDeleteTramite()
		{
			ActualizarListaFiltrada();
			await Task.Delay(1);
			StateHasChanged();
		}

		#region FILTROS
		private async Task CuandoCambiaTexto(string nuevoTexto)
		{
			filtroTexto = nuevoTexto;
			ActualizarListaFiltrada();
			StateHasChanged();
		}



		private void ActualizarListaFiltrada()
		{
			if (string.IsNullOrEmpty(filtroTexto))
			{
				listaFiltrada = Data.LstTramites.ToList();
				return;
			}
			listaFiltrada = Data.LstTramites
				.Where(tramite => tramite.vchNombre.ToLower().Contains(filtroTexto.ToLower()))
				.ToList();
		}
		#endregion
	}
}
