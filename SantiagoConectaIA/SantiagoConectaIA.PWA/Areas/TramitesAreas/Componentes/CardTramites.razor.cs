using EngramaCoreStandar.Extensions;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using SantiagoConectaIA.PWA.Areas.TramitesAreas.Utiles;
using SantiagoConectaIA.PWA.Shared.Common;
using SantiagoConectaIA.Share.Objects.TramitesModule;

namespace SantiagoConectaIA.PWA.Areas.TramitesAreas.Componentes
{
	public partial class CardTramites : EngramaComponent
	{
		#region PARAMETROS
		[Parameter] public DataTramites Data { get; set; }
		[Parameter] public Tramite Tramite { get; set; }
		[Parameter] public EventCallback<Tramite> OnTramiteSelected { get; set; }
		[Parameter] public EventCallback<Tramite> OnDeleteTramite { get; set; }
		#endregion

		#region CICLO VIDA BLAZOR
		protected override async Task OnInitializedAsync()
		{

		}
		#endregion

		private async void OnClickTramiteSelected()
		{
			await OnTramiteSelected.InvokeAsync(Tramite);
		}
		private async void OnCLickEliminarTramite(Tramite tramite)
		{
			var parameters = new DialogParameters { { "ContentText", "¿Estás seguro de eliminar este trámite?" } };
			var dialog = DialogService.Show<ConfirmationDialog>("Confirmar eliminación", parameters);
			var result = await dialog.Result;

			if (result.Canceled.False())
			{
				Loading.Show();
				Tramite.bActivo = false;
				Data.TramiteSelected = Tramite;
				var saveResult = await Data.PostSaveTramites();
				ShowSnake(saveResult);
				if (saveResult.bResult)
				{
					await OnDeleteTramite.InvokeAsync(Tramite);
				}
				Loading.Hide();
			}
		}
	}
}
