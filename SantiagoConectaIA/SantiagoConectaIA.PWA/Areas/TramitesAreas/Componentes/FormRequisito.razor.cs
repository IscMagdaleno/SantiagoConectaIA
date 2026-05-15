using Microsoft.AspNetCore.Components;
using SantiagoConectaIA.PWA.Shared.Common;
using SantiagoConectaIA.Share.Objects.TramitesModule;
using MudBlazor;
using SantiagoConectaIA.PWA.Shared.Workspace;

namespace SantiagoConectaIA.PWA.Areas.TramitesAreas.Componentes
{
	public partial class FormRequisito : EngramaComponent
	{
		[Parameter] public Tramite TramiteModel { get; set; }
		[Parameter] public TipoEstadoControl EstadoControl { get; set; }

		private Requisitos nuevoRequisito = new Requisitos();

		private void AgregarRequisito()
		{
			if (string.IsNullOrWhiteSpace(nuevoRequisito.vchNombre))
			{
				Snackbar.Add("El nombre del requisito es obligatorio.", Severity.Warning);
				return;
			}
			
			if (TramiteModel.Requisitos == null)
				TramiteModel.Requisitos = new List<Requisitos>();

			TramiteModel.Requisitos.Add(new Requisitos
			{
				vchNombre = nuevoRequisito.vchNombre,
				nvchDetalle = nuevoRequisito.nvchDetalle,
				bObligatorio = nuevoRequisito.bObligatorio
			});
			nuevoRequisito = new Requisitos();
		}

		private void EliminarRequisito(Requisitos req)
		{
			TramiteModel.Requisitos.Remove(req);
		}
	}
}
