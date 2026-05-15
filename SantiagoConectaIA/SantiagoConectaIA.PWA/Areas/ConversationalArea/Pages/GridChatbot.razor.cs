using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using EngramaCoreStandar.Results;
using EngramaCoreStandar.Dapper.Results;
using SantiagoConectaIA.PWA.Areas.ConversationalArea.Utiles;
using SantiagoConectaIA.PWA.Shared.Common;
using SantiagoConectaIA.Share.Objects.ConversationalModule;
using SantiagoConectaIA.PWA.Shared.Workspace;

namespace SantiagoConectaIA.PWA.Areas.ConversationalArea.Pages
{
	public partial class GridChatbot : EngramaWorkspaceComponent
	{
		[Inject] public MainConversational Data { get; set; }
		[Inject] public EngramaCoreStandar.Servicios.IHttpService httpService { get; set; }
		[Inject] public EngramaCoreStandar.Servicios.IValidaServicioService validaServicioService { get; set; }

		public Chat SelectedChat { get; set; }
		public string FiltroTexto { get; set; } = string.Empty;
		public bool IsLoadingMessages { get; set; } = false;

		public IEnumerable<Chat> ChatsFiltrados =>
			string.IsNullOrWhiteSpace(FiltroTexto)
				? Data.LstChats
				: Data.LstChats.Where(c => (c.nvchNombre?.Contains(FiltroTexto, StringComparison.OrdinalIgnoreCase) ?? false) ||
										   (c.nvchThreadId?.Contains(FiltroTexto, StringComparison.OrdinalIgnoreCase) ?? false));

		protected override async Task OnInitializedAsync()
		{
			await LoadChats();
		}

		public async Task LoadChats()
		{
			try
			{
				Loading.Show();
				var res = await Data.PostGetChats(1); // Proyecto 1 por defecto para Santiago Papasquiaro
				ShowSnake(res);
			}
			finally
			{
				Loading.Hide();
			}
		}

		public async Task SelectChat(Chat chat)
		{
			SelectedChat = chat;
			IsLoadingMessages = true;
			StateHasChanged();

			try
			{
				var res = await Data.PostGetMensajes(chat.iIdChat);
				ShowSnake(res);
			}
			finally
			{
				IsLoadingMessages = false;
				StateHasChanged();
			}
		}

		public async Task RefreshCurrentChat()
		{
			if (SelectedChat != null)
			{
				await SelectChat(SelectedChat);
			}
		}

		public async Task SeedMockData()
		{
			try
			{
				Loading.Show();
				var res = await httpService.Post<object, Response<object>>("api/Chat/PostSeedMockData", new { });
				var validation = validaServicioService.ValidadionServicio(res, onSuccess: async data =>
				{
					await LoadChats();
				});
				ShowSnake(validation);
			}
			catch (Exception ex)
			{
				ShowSnake(new SeverityMessage(false, $"Excepción: {ex.Message}", SeverityTag.Error));
			}
			finally
			{
				Loading.Hide();
			}
		}

		protected override List<MenuItemModel> GetMenuItems()
		{
			var items = new List<MenuItemModel>();

			items.Add(new MenuItemModel
			{
				Text = "Actualizar Listado",
				Icon = MudBlazor.Icons.Material.Filled.Refresh,
				Color = MudBlazor.Color.Info,
				Action = EventCallback.Factory.Create(this, async () => { await LoadChats(); })
			});

			if (Data?.LstChats != null && !Data.LstChats.Any())
			{
				items.Add(new MenuItemModel
				{
					Text = "Generar Datos de Prueba",
					Icon = MudBlazor.Icons.Material.Filled.AutoAwesome,
					Color = MudBlazor.Color.Primary,
					Action = EventCallback.Factory.Create(this, async () => { await SeedMockData(); })
				});
			}

			return items;
		}
	}
}
