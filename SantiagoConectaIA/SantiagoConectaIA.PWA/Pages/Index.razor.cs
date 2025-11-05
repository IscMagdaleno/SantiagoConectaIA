using Microsoft.AspNetCore.Components;

namespace SantiagoConectaIA.PWA.Pages
{
	public partial class Index : ComponentBase
	{
		[Inject] private NavigationManager Navigation { get; set; } = default!;

		/// <summary>
		/// Navega a la página especificada
		/// </summary>
		/// <param name="url">URL de destino</param>
		private void NavigateToPage(string url)
		{
			Navigation.NavigateTo(url);
		}

		/// <summary>
		/// Se ejecuta cuando el componente se inicializa
		/// </summary>
		protected override async Task OnInitializedAsync()
		{
			await base.OnInitializedAsync();
			// Aquí se pueden cargar datos iniciales si es necesario
		}
	}
}
