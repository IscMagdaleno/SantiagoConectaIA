using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace SantiagoConectaIA.PWA.Shared.Workspace
{
	public class MenuItemModel
	{
		public string Text { get; set; } = string.Empty;
		public string Icon { get; set; } = string.Empty;
		public EventCallback Action { get; set; }
		public Color Color { get; set; } = Color.Primary;
	}
}
