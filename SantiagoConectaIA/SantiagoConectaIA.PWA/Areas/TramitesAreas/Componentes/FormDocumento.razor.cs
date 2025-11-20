using Microsoft.AspNetCore.Components;

using SantiagoConectaIA.PWA.Areas.TramitesAreas.Utiles;
using SantiagoConectaIA.PWA.Shared.Common;
using SantiagoConectaIA.Share.Objects.TramitesModule;

namespace SantiagoConectaIA.PWA.Areas.TramitesAreas.Componentes
{
	public partial class FormDocumento : EngramaComponent
	{


		[Parameter] public MainTramites Data { get; set; }
		[Parameter] public EventCallback<Documento> OnDocumentoSaved { get; set; }

		private async Task OnSubmint()
		{
			Loading.Show();

			var result = await Data.PostSaveDocumento();
			ShowSnake(result);
			if (result.bResult)
			{
				await OnDocumentoSaved.InvokeAsync(Data.DocumentoSelected);
			}
			Loading.Hide();

		}
	}
}
