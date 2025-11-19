using EngramaCoreStandar.Extensions;

using Microsoft.AspNetCore.Components;

using MudBlazor;

using SantiagoConectaIA.PWA.Areas.TramitesAreas.Utiles;
using SantiagoConectaIA.PWA.Shared.Common;
using SantiagoConectaIA.Share.Objects.OficinasModule;

namespace SantiagoConectaIA.PWA.Areas.TramitesAreas.Componentes
{
	public partial class CardOficina
	{

		[Parameter] public MainTramites Data { get; set; }
		[Parameter] public Oficina Oficina { get; set; }
		[Parameter] public EventCallback<Oficina> OnOficinaSelected { get; set; }
		[Parameter] public EventCallback<Oficina> OnDeleteOficina { get; set; }


		private async void OnClickOficinaSelected()
		{
			await OnOficinaSelected.InvokeAsync(Oficina);
		}


		private async void OnCLickEliminarOficina(Oficina oficina)
		{
			var parameters = new DialogParameters { { "ContentText", "¿Estás seguro de eliminar esta Oficina?" } };
			var dialog = await DialogService.ShowAsync<ConfirmationDialog>("Confirmar eliminación", parameters);
			var result = await dialog.Result;

			if (result.Canceled.False())
			{
				Loading.Show();
				Oficina.bActivo = false;
				Data.OficinaSelected = Oficina;
				var saveResult = await Data.PostSaveOficina();
				ShowSnake(saveResult);
				if (saveResult.bResult)
				{
					Data.LstOficinas = Data.LstOficinas.Where(e => e.iIdOficina != oficina.iIdOficina).ToList();

					await OnDeleteOficina.InvokeAsync(Oficina);
				}
				Loading.Hide();
			}
		}
	}
}
