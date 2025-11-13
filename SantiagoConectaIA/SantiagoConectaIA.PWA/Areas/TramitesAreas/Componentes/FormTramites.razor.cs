using Microsoft.AspNetCore.Components;

using SantiagoConectaIA.PWA.Areas.TramitesAreas.Utiles;
using SantiagoConectaIA.PWA.Shared.Common;

namespace SantiagoConectaIA.PWA.Areas.TramitesAreas.Componentes
{
	public partial class FormTramites : EngramaComponent
	{
		#region PARAMETROS
		[Parameter] public DataTramites Data { get; set; }
		[Parameter] public EventCallback OnTramiteSaved { get; set; }
		#endregion

		#region CICLO VIDA BLAZOR
		protected override void OnInitialized()
		{

		}

		protected override async Task OnInitializedAsync()
		{
			await Data.PostGetOficinas();

		}
		#endregion

		private async Task OnSubmit()
		{

			Loading.Show();
			var result = await Data.PostSaveTramite();

			ShowSnake(result);
			if (result.bResult)
			{
				await OnTramiteSaved.InvokeAsync();
			}

			Loading.Hide();

		}
	}
}
