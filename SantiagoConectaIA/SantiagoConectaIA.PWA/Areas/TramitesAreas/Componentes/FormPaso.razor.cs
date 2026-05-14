using Microsoft.AspNetCore.Components;
using SantiagoConectaIA.PWA.Shared.Common;
using SantiagoConectaIA.Share.Objects.TramitesModule;
using MudBlazor;

namespace SantiagoConectaIA.PWA.Areas.TramitesAreas.Componentes
{
	public partial class FormPaso : EngramaComponent
	{
		[Parameter] public Tramite TramiteModel { get; set; }

		private Pasos nuevoPaso = new Pasos();

		private void AgregarPaso()
		{
			if (string.IsNullOrWhiteSpace(nuevoPaso.nvchDescripcion))
			{
				Snackbar.Add("La descripción del paso es obligatoria.", Severity.Warning);
				return;
			}

			if (TramiteModel.Pasos == null)
				TramiteModel.Pasos = new List<Pasos>();

			var maxOrden = TramiteModel.Pasos.Any() ? TramiteModel.Pasos.Max(p => p.iOrden) : 0;
			TramiteModel.Pasos.Add(new Pasos
			{
				nvchDescripcion = nuevoPaso.nvchDescripcion,
				iOrden = (short)(maxOrden + 1)
			});
			nuevoPaso = new Pasos();
		}

		private void EliminarPaso(Pasos paso)
		{
			TramiteModel.Pasos.Remove(paso);
			ReordenarPasos();
		}

		private void MoverPaso(Pasos paso, int direccion)
		{
			var idx = TramiteModel.Pasos.IndexOf(paso);
			if (idx < 0) return;
			
			var newIdx = idx + direccion;
			if (newIdx >= 0 && newIdx < TramiteModel.Pasos.Count)
			{
				var temp = TramiteModel.Pasos[newIdx];
				TramiteModel.Pasos[newIdx] = paso;
				TramiteModel.Pasos[idx] = temp;
				ReordenarPasos();
			}
		}

		private void ReordenarPasos()
		{
			for (int i = 0; i < TramiteModel.Pasos.Count; i++)
			{
				TramiteModel.Pasos[i].iOrden = (short)(i + 1);
			}
		}
	}
}
