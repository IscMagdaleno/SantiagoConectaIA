using Microsoft.AspNetCore.Components;

using SantiagoConectaIA.PWA.Areas.TramitesAreas.Utiles;
using SantiagoConectaIA.PWA.Shared.Common;
using SantiagoConectaIA.Share.Objects.TramitesModule;

using SantiagoConectaIA.PWA.Shared.Workspace;

namespace SantiagoConectaIA.PWA.Areas.TramitesAreas.Componentes
{
	public partial class GridTramites : EngramaWorkspaceComponent
	{

		[Inject] public MainTramites Data { get; set; }


		public bool bShowFormTramite;

		private string filtroTexto = "";
		private List<Tramite> listaFiltrada = new List<Tramite>();

		#region CICLO VIDA BLAZOR
		protected override void OnInitialized()
		{

		}
		protected override async Task OnInitializedAsync()
		{

			Loading.Show();
			ShowSnake(await Data.PostGetTramites());
			ActualizarListaFiltrada();
			Loading.Hide();
		}
		#endregion


		private async Task OnTramiteView(Tramite tramite)
		{
			Loading.Show();
			ShowSnake(await Data.PostGetTramiteDetalle(tramite.iIdTramite));
			
			var type = typeof(FormTramites);
			var tab = new WorkspaceTab
			{
				Icono = MudBlazor.Icons.Material.Filled.Visibility,
				Text = $"Trámite {tramite.iIdTramite}",
				TipoControl = type,
				Repetir = true,
				EstadoControl = TipoEstadoControl.Lectura
			};

			tab.Componente = builder =>
			{
				builder.OpenComponent(0, type);
				builder.AddComponentReferenceCapture(1, instance =>
				{
					if (instance is EngramaWorkspaceComponent baseComponent)
					{
						tab.InstanciaComponente = baseComponent;
						baseComponent.IconoBase = tab.Icono ?? "";
						baseComponent.EstadoControl = tab.EstadoControl;
						if (baseComponent is FormTramites form)
						{
							form.OnTramiteSaved = EventCallback.Factory.Create(this, OnTramiteSaved);
						}
						baseComponent.TriggerMenuUpdate();
					}
				});
				builder.CloseComponent();
			};

			AgregarTab(tab);
			Loading.Hide();
		}

		private async Task OnTramiteEdit(Tramite tramite)
		{
			Loading.Show();
			ShowSnake(await Data.PostGetTramiteDetalle(tramite.iIdTramite));
			
			var type = typeof(FormTramites);
			var tab = new WorkspaceTab
			{
				Icono = MudBlazor.Icons.Material.Filled.Edit,
				Text = $"Trámite {tramite.iIdTramite}",
				TipoControl = type,
				Repetir = true,
				EstadoControl = TipoEstadoControl.Edicion
			};

			tab.Componente = builder =>
			{
				builder.OpenComponent(0, type);
				builder.AddComponentReferenceCapture(1, instance =>
				{
					if (instance is EngramaWorkspaceComponent baseComponent)
					{
						tab.InstanciaComponente = baseComponent;
						baseComponent.IconoBase = tab.Icono ?? "";
						baseComponent.EstadoControl = tab.EstadoControl;
						if (baseComponent is FormTramites form)
						{
							form.OnTramiteSaved = EventCallback.Factory.Create(this, OnTramiteSaved);
						}
						baseComponent.TriggerMenuUpdate();
					}
				});
				builder.CloseComponent();
			};

			AgregarTab(tab);
			Loading.Hide();
		}

		private void OnClickShowForm()
		{
			Data.TramiteSelected = new();
			var type = typeof(FormTramites);
			var tab = new WorkspaceTab
			{
				Icono = MudBlazor.Icons.Material.Filled.NoteAdd,
				Text = "Nuevo Trámite",
				TipoControl = type,
				Repetir = false,
				EstadoControl = TipoEstadoControl.Alta
			};

			tab.Componente = builder =>
			{
				builder.OpenComponent(0, type);
				builder.AddComponentReferenceCapture(1, instance =>
				{
					if (instance is EngramaWorkspaceComponent baseComponent)
					{
						tab.InstanciaComponente = baseComponent;
						baseComponent.IconoBase = tab.Icono ?? "";
						baseComponent.EstadoControl = tab.EstadoControl;
						if (baseComponent is FormTramites form)
						{
							form.OnTramiteSaved = EventCallback.Factory.Create(this, OnTramiteSaved);
						}
						baseComponent.TriggerMenuUpdate();
					}
				});
				builder.CloseComponent();
			};

			AgregarTab(tab);
		}

		protected override List<MenuItemModel> GetMenuItems()
		{
			return new List<MenuItemModel>
			{
				new MenuItemModel
				{
					Text = "Agregar Nuevo",
					Icon = MudBlazor.Icons.Material.Filled.Add,
					Color = MudBlazor.Color.Success,
					Action = EventCallback.Factory.Create(this, OnClickShowForm)
				},
				new MenuItemModel
				{
					Text = "Actualizar",
					Icon = MudBlazor.Icons.Material.Filled.Refresh,
					Color = MudBlazor.Color.Info,
					Action = EventCallback.Factory.Create(this, async () => {
						Loading.Show();
						ShowSnake(await Data.PostGetTramites());
						ActualizarListaFiltrada();
						Loading.Hide();
					})
				}
			};
		}

		public async Task OnTramiteSaved()
		{
			ShowSnake(await Data.PostGetTramites());
			ActualizarListaFiltrada();
			StateHasChanged();
		}

		private async Task OnDeleteTramite()
		{
			ActualizarListaFiltrada();
			await Task.Delay(1);
			StateHasChanged();
		}

		#region FILTROS
		private async Task CuandoCambiaTexto(string nuevoTexto)
		{
			filtroTexto = nuevoTexto;
			ActualizarListaFiltrada();
			StateHasChanged();
		}



		private void ActualizarListaFiltrada()
		{
			if (string.IsNullOrEmpty(filtroTexto))
			{
				listaFiltrada = Data.LstTramites.ToList();
				return;
			}
			listaFiltrada = Data.LstTramites
				.Where(tramite => tramite.vchNombre.ToLower().Contains(filtroTexto.ToLower()))
				.ToList();
		}
		#endregion
	}
}
