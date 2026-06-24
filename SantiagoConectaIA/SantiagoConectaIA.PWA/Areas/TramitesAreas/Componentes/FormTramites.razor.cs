using Microsoft.AspNetCore.Components;
using SantiagoConectaIA.PWA.Areas.TramitesAreas.Utiles;
using SantiagoConectaIA.PWA.Shared.Common;
using SantiagoConectaIA.Share.Objects.TramitesModule;
using MudBlazor;

using SantiagoConectaIA.PWA.Shared.Workspace;

namespace SantiagoConectaIA.PWA.Areas.TramitesAreas.Componentes
{
	public partial class FormTramites : EngramaWorkspaceComponent
	{
		#region PARAMETROS
		[Inject] public MainTramites Data { get; set; }
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
			if (TramiteModel.bPrecioCalculado)
			{
				TramiteModel.mCosto = 0;
			}

			Loading.Show();
			var result = await Data.PostSaveTramite(TramiteModel);
			ShowSnake(result);
			if (result.bResult)
			{
				// Sincronizar el ID devuelto por el servidor
				TramiteModel.iIdTramite = Data.TramiteSelected.iIdTramite;

				// Pasar a modo Lectura después de guardar
				EstadoControl = TipoEstadoControl.Lectura;
				
				// Actualizar el nombre del tab con el nuevo ID si era un alta
				SetNombreTab($"Trámite {TramiteModel.vchNombre}");
				
				TriggerMenuUpdate();
				await OnTramiteSaved.InvokeAsync();
			}
			Loading.Hide();
		}

		protected override List<MenuItemModel> GetMenuItems()
		{
			var items = new List<MenuItemModel>();

			if (EstadoControl == TipoEstadoControl.Lectura)
			{
				items.Add(new MenuItemModel
				{
					Text = "Editar",
					Icon = MudBlazor.Icons.Material.Filled.Edit,
					Color = MudBlazor.Color.Primary,
					Action = EventCallback.Factory.Create(this, () => {
						EstadoControl = TipoEstadoControl.Edicion;
						TriggerMenuUpdate();
					})
				});
				items.Add(new MenuItemModel
				{
					Text = "Cerrar",
					Icon = MudBlazor.Icons.Material.Filled.Close,
					Color = MudBlazor.Color.Error,
					Action = EventCallback.Factory.Create(this, CerrarTab)
				});
			}
			else
			{
				items.Add(new MenuItemModel
				{
					Text = "Guardar",
					Icon = MudBlazor.Icons.Material.Filled.Save,
					Color = MudBlazor.Color.Success,
					Action = EventCallback.Factory.Create(this, OnSubmit)
				});
				items.Add(new MenuItemModel
				{
					Text = "Cerrar",
					Icon = MudBlazor.Icons.Material.Filled.Close,
					Color = MudBlazor.Color.Error,
					Action = EventCallback.Factory.Create(this, CerrarTab)
				});
			}

			return items;
		}
	}
}
