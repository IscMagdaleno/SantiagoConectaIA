using Microsoft.AspNetCore.Components;
using SantiagoConectaIA.PWA.Shared.Common;
using SantiagoConectaIA.Share.Objects.TramitesModule;
using MudBlazor;

namespace SantiagoConectaIA.PWA.Areas.TramitesAreas.Componentes
{
	public partial class FormDocumento : EngramaComponent
	{
		[Parameter] public Tramite TramiteModel { get; set; }

		private Documento nuevoDocumento = new Documento();

		private void AgregarDocumento()
		{
			if (string.IsNullOrWhiteSpace(nuevoDocumento.vchNombre) || string.IsNullOrWhiteSpace(nuevoDocumento.vchUrlDocumento))
			{
				Snackbar.Add("Nombre y Enlace son obligatorios.", Severity.Warning);
				return;
			}
			
			if (TramiteModel.Documentos == null)
				TramiteModel.Documentos = new List<Documento>();

			TramiteModel.Documentos.Add(new Documento
			{
				vchNombre = nuevoDocumento.vchNombre,
				vchUrlDocumento = nuevoDocumento.vchUrlDocumento
			});
			nuevoDocumento = new Documento();
		}

		private void EliminarDocumento(Documento doc)
		{
			TramiteModel.Documentos.Remove(doc);
		}
	}
}
