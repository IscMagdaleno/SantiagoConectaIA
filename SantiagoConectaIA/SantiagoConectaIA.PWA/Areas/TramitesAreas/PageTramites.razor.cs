using EngramaCoreStandar.Mapper;
using EngramaCoreStandar.Servicios;
using Microsoft.AspNetCore.Components;
using SantiagoConectaIA.PWA.Areas.TramitesAreas.Utiles;
using SantiagoConectaIA.PWA.Shared.Common;
using SantiagoConectaIA.Share.Objects.TramitesModule;
using System.Threading.Tasks;

namespace SantiagoConectaIA.PWA.Areas.TramitesAreas
{
	public partial class PageTramites : EngramaPage
	{

		#region INJECTS
		[Inject] public DataTramites Data { get; set; }
		#endregion

		#region PROPIEDADES
		public bool bShowFormTramite;
		//FILTRO DE TRÁMITES
		private string filtroTexto = "";
		private List<Tramite> listaFiltrada = new List<Tramite>();
		#endregion

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


		private void OnClickShowForm()
		{
			Data.TramiteSelected = new();
			bShowFormTramite = true;
		}
		private void OnClickShowData()
		{
			bShowFormTramite = false;
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
