using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using EngramaCoreStandar.Results;
using SantiagoConectaIA.Share.Objects.WhatsAppModule;

namespace SantiagoConectaIA.PWA.Areas.PageVisitsArea.Utiles
{
	public class MainWhatsAppAnalytics
	{
		private readonly HttpClient _httpClient;

		public MainWhatsAppAnalytics(HttpClient httpClient)
		{
			_httpClient = httpClient;
		}

		public async Task<Response<WhatsAppStats>> GetStats()
		{
			try
			{
				var response = await _httpClient.GetAsync("api/WhatsAppAnalytics/stats");
				if (response.IsSuccessStatusCode)
				{
					return await response.Content.ReadFromJsonAsync<Response<WhatsAppStats>>()
						?? Response<WhatsAppStats>.BadResult("Respuesta vacía", new WhatsAppStats());
				}

				return Response<WhatsAppStats>.BadResult("Error al consultar estadísticas de WhatsApp", new WhatsAppStats());
			}
			catch (Exception ex)
			{
				return Response<WhatsAppStats>.BadResult(ex.Message, new WhatsAppStats());
			}
		}

		public async Task<Response<IEnumerable<WhatsAppDailyStats>>> GetDailyStats(int days = 30)
		{
			try
			{
				var response = await _httpClient.GetAsync($"api/WhatsAppAnalytics/daily?days={days}");
				if (response.IsSuccessStatusCode)
				{
					return await response.Content.ReadFromJsonAsync<Response<IEnumerable<WhatsAppDailyStats>>>()
						?? Response<IEnumerable<WhatsAppDailyStats>>.BadResult("Respuesta vacía", Enumerable.Empty<WhatsAppDailyStats>());
				}

				return Response<IEnumerable<WhatsAppDailyStats>>.BadResult(
					"Error al consultar estadísticas diarias de WhatsApp",
					Enumerable.Empty<WhatsAppDailyStats>());
			}
			catch (Exception ex)
			{
				return Response<IEnumerable<WhatsAppDailyStats>>.BadResult(ex.Message, Enumerable.Empty<WhatsAppDailyStats>());
			}
		}

		public async Task<Response<IEnumerable<WhatsAppUser>>> GetUsers(int top = 100)
		{
			try
			{
				var response = await _httpClient.GetAsync($"api/WhatsAppAnalytics/users?top={top}");
				if (response.IsSuccessStatusCode)
				{
					return await response.Content.ReadFromJsonAsync<Response<IEnumerable<WhatsAppUser>>>()
						?? Response<IEnumerable<WhatsAppUser>>.BadResult("Respuesta vacía", Enumerable.Empty<WhatsAppUser>());
				}

				return Response<IEnumerable<WhatsAppUser>>.BadResult(
					"Error al consultar usuarios de WhatsApp",
					Enumerable.Empty<WhatsAppUser>());
			}
			catch (Exception ex)
			{
				return Response<IEnumerable<WhatsAppUser>>.BadResult(ex.Message, Enumerable.Empty<WhatsAppUser>());
			}
		}
	}
}
