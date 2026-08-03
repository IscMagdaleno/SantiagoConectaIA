using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using SantiagoConectaIA.Share.Objects.AnalyticsModule;
using SantiagoConectaIA.Share.PostModels.AnalyticsModule;

namespace SantiagoConectaIA.PWA.Areas.PageVisitsArea.Utiles
{
	public class MainAnalytics
	{
		private readonly HttpClient _httpClient;

		public MainAnalytics(HttpClient httpClient)
		{
			_httpClient = httpClient;
		}

		public async Task<IEnumerable<PageVisitSummary>> GetPageVisitsSummary(DateTime? startDate, DateTime? endDate)
		{
			try
			{
				var response = await _httpClient.PostAsJsonAsync(
					"api/Analytics/PostGetPageVisitsSummary",
					new PostAnalyticsDateRange { dtStartDate = startDate, dtEndDate = endDate });

				if (response.IsSuccessStatusCode)
				{
					return await response.Content.ReadFromJsonAsync<IEnumerable<PageVisitSummary>>()
						?? Enumerable.Empty<PageVisitSummary>();
				}
			}
			catch
			{
				// Swallow and return empty — dashboard shows empty state
			}

			return Enumerable.Empty<PageVisitSummary>();
		}

		public async Task<IEnumerable<PageVisitByPage>> GetPageVisitsByPage(DateTime? startDate, DateTime? endDate)
		{
			try
			{
				var response = await _httpClient.PostAsJsonAsync(
					"api/Analytics/PostGetPageVisitsByPage",
					new PostAnalyticsDateRange { dtStartDate = startDate, dtEndDate = endDate });

				if (response.IsSuccessStatusCode)
				{
					return await response.Content.ReadFromJsonAsync<IEnumerable<PageVisitByPage>>()
						?? Enumerable.Empty<PageVisitByPage>();
				}
			}
			catch
			{
			}

			return Enumerable.Empty<PageVisitByPage>();
		}

		public async Task<IEnumerable<DailyTraffic>> GetDailyTraffic(DateTime? startDate, DateTime? endDate)
		{
			try
			{
				var response = await _httpClient.PostAsJsonAsync(
					"api/Analytics/PostGetDailyTraffic",
					new PostAnalyticsDateRange { dtStartDate = startDate, dtEndDate = endDate });

				if (response.IsSuccessStatusCode)
				{
					return await response.Content.ReadFromJsonAsync<IEnumerable<DailyTraffic>>()
						?? Enumerable.Empty<DailyTraffic>();
				}
			}
			catch
			{
			}

			return Enumerable.Empty<DailyTraffic>();
		}

		public async Task<IEnumerable<PageVisit>> GetRecentVisits(int topRows = 20)
		{
			try
			{
				var response = await _httpClient.PostAsJsonAsync(
					"api/Analytics/PostGetRecentVisits",
					new PostAnalyticsTopRows { iTopRows = topRows });

				if (response.IsSuccessStatusCode)
				{
					return await response.Content.ReadFromJsonAsync<IEnumerable<PageVisit>>()
						?? Enumerable.Empty<PageVisit>();
				}
			}
			catch
			{
			}

			return Enumerable.Empty<PageVisit>();
		}
	}
}
