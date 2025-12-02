using EngramaCoreStandar.Dapper.Results;

using Microsoft.AspNetCore.Components;

using MudBlazor;

using SantiagoConectaIA.PWA.Areas.TramitesAreas.Utiles;
using SantiagoConectaIA.PWA.Shared.Common;

namespace SantiagoConectaIA.PWA.Areas.TramitesAreas.Componentes
{
	public partial class WizardTramites : EngramaComponent
	{

		[Parameter] public MainTramites Data { get; set; } // Ajustar el tipo de Main

		// Referencia al componente MudStepper para controlar el avance
		private MudStepper Stepper { get; set; }


		public async Task OnTramiteSaved()
		{

			// 3. Mostrar notificación de éxito (usando EngramaComponent.ShowSnake)
			var message = new SeverityMessage(true, "Trámite inicial registrado. Puede continuar con los documentos.", SeverityTag.Success);
			ShowSnake(message);

			// 4. Mover al siguiente paso (Documentos Requeridos)
			await Stepper.NextStepAsync();
		}

		public async Task OnRequisitoSaved()
		{

			await Task.Delay(1);
			StateHasChanged();
		}

		// Se ejecuta al guardar exitosamente los documentos
		public async Task OnDocumentoSaved()
		{

			await Task.Delay(1);
			StateHasChanged();

		}

	}
}
