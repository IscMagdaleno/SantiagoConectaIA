using Microsoft.AspNetCore.Components;

using SantiagoConectaIA.PWA.Areas.TramitesAreas.Utiles;
using SantiagoConectaIA.PWA.Shared.Common;
using SantiagoConectaIA.Share.Objects.TramitesModule;

namespace SantiagoConectaIA.PWA.Areas.TramitesAreas.Componentes
{
	public partial class FormRequisito : EngramaComponent
	{

		[Parameter] public MainTramites Data { get; set; }
		[Parameter] public EventCallback<Requisitos> OnRequisitoSaved { get; set; }

		private async Task OnSubmint()
		{
			Loading.Show();

			var result = await Data.PostSaveRequisito();
			ShowSnake(result);
			if (result.bResult)
			{
				await OnRequisitoSaved.InvokeAsync(Data.RequisitoSelected);
			}
			Loading.Hide();

		}
	}
}
