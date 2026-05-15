using System;
using System.Collections.Generic;

namespace SantiagoConectaIA.PWA.Shared.Workspace
{
	public class WorkspaceService
	{
		// Evento para agregar un tab
		public event Action<WorkspaceTab> OnMenuTabSelected;
		public void AddComponenteTab(WorkspaceTab tab)
		{
			OnMenuTabSelected?.Invoke(tab);
		}

		// Evento para el Menu Superior
		public event Action MenuChanged;
		private List<MenuItemModel> _currentMenuItems = new List<MenuItemModel>();
		public IReadOnlyList<MenuItemModel> CurrentMenuItems => _currentMenuItems;
		public void SetMenuItems(List<MenuItemModel> menuItems)
		{
			_currentMenuItems = menuItems;
			MenuChanged?.Invoke();
		}

		// Evento para cambiar el nombre de un tab (si es necesario)
		public event Action<string, string> TabNameChanged;
		public void SetCurrentTabName(string tabName, string iconText)
		{
			TabNameChanged?.Invoke(tabName, iconText);
		}

		public event Action<TipoEstadoControl> TabStateChanged;
		public void SetCurrentTabState(TipoEstadoControl tabState)
		{
			TabStateChanged?.Invoke(tabState);
		}

		// Evento para cerrar tab
		public event Action<bool> CloseCurrentTab;
		public void SetCloseCurrentTab(bool withWarning = false)
		{
			CloseCurrentTab?.Invoke(withWarning);
		}

        // Eventos de solicitud de módulos
        public event Action OnTramitesRequest;
        public void RequestTramites() => OnTramitesRequest?.Invoke();

        public event Action OnNoticiasRequest;
        public void RequestNoticias() => OnNoticiasRequest?.Invoke();

        public event Action OnChatbotRequest;
        public void RequestChatbot() => OnChatbotRequest?.Invoke();

        public event Action OnOficinasRequest;
        public void RequestOficinas() => OnOficinasRequest?.Invoke();
	}
}
