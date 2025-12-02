using Microsoft.AspNetCore.Components;

using SantiagoConectaIA.PWA.Areas.TramitesAreas.Utiles;
using SantiagoConectaIA.PWA.Shared.Common;
using SantiagoConectaIA.Share.Objects.TramitesModule;

namespace SantiagoConectaIA.PWA.Areas.TramitesAreas.Componentes
{
	public partial class FormTramites : EngramaComponent
	{
		#region PARAMETROS
		[Parameter] public MainTramites Data { get; set; }
		[Parameter] public EventCallback OnTramiteSaved { get; set; }
		#endregion

		#region CICLO VIDA BLAZOR

		private Tramite TramiteModel { get; set; }

		protected override void OnInitialized()
		{
			if (Data.TramiteSelected.iIdTramite <= 0)
			{
				TramiteModel = new Tramite();
			}
			else
			{
				TramiteModel = Data.TramiteSelected;
			}
		}

		protected override async Task OnInitializedAsync()
		{
			await Data.PostGetOficinas();

		}
		#endregion

		private async Task OnSubmit()
		{

			Loading.Show();

			var result = await Data.PostSaveTramite(TramiteModel);

			ShowSnake(result);
			if (result.bResult)
			{
				await OnTramiteSaved.InvokeAsync();
			}

			Loading.Hide();

		}
	}
}
