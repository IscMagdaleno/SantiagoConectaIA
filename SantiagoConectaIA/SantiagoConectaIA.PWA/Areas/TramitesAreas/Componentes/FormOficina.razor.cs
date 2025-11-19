using Microsoft.AspNetCore.Components;

using SantiagoConectaIA.PWA.Areas.TramitesAreas.Utiles;
using SantiagoConectaIA.PWA.Shared.Common;
using SantiagoConectaIA.Share.Objects.OficinasModule;

namespace SantiagoConectaIA.PWA.Areas.TramitesAreas.Componentes
{
	public partial class FormOficina : EngramaComponent
	{


		[Parameter] public MainTramites Data { get; set; }
		[Parameter] public EventCallback<Oficina> OnOficinaSaved { get; set; }

		protected override async Task OnInitializedAsync()
		{

			Loading.Show();
			Loading.Hide();
		}


		private async Task OnSubmint()
		{
			Loading.Show();

			var result = await Data.PostSaveOficina();
			ShowSnake(result);
			if (result.bResult)
			{
				await OnOficinaSaved.InvokeAsync(Data.OficinaSelected);
			}
			Loading.Hide();

		}
	}
}
