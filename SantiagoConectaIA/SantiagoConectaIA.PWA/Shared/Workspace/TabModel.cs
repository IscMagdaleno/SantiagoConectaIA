using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;

namespace SantiagoConectaIA.PWA.Shared.Workspace
{
	public class WorkspaceTab
	{
		public Guid Id { get; set; } = Guid.NewGuid();
		public TipoEstadoControl EstadoControl { get; set; } = TipoEstadoControl.Lectura;
		public RenderFragment Componente { get; set; }
		public string Icono { get; set; }
		public EngramaWorkspaceComponent InstanciaComponente { get; set; }
		public List<MenuItemModel> OpcionesMenu { get; set; } = new List<MenuItemModel>();
		public string Text { get; set; }
		public Type TipoControl { get; set; }
		public bool Repetir { get; set; }
	}
}
