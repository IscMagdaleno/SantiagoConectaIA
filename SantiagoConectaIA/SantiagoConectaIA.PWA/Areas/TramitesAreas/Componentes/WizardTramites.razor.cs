using EngramaCoreStandar.Dapper.Results;

using Microsoft.AspNetCore.Components;

using MudBlazor;

using SantiagoConectaIA.PWA.Areas.TramitesAreas.Utiles;
using SantiagoConectaIA.PWA.Shared.Common;

namespace SantiagoConectaIA.PWA.Areas.TramitesAreas.Componentes
{
	public partial class WizardTramites : EngramaComponent
	{
		[Parameter] public MainTramites Data { get; set; }

		public async Task OnTramiteSaved()
		{
			// Si es necesario ejecutar alguna lógica post-guardado
			Snackbar.Add("Trámite guardado correctamente", Severity.Success);
			StateHasChanged();
			await Task.CompletedTask;
		}
	}
}
