using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using EngramaCoreStandar.Results;
using SantiagoConectaIA.Share.Objects.PageVisitsModule;
using SantiagoConectaIA.Share.PostModels.PageVisitsModule;

namespace SantiagoConectaIA.PWA.Areas.PageVisitsArea.Utiles
{
	public class MainPageVisits
	{
		private readonly HttpClient _httpClient;

		public MainPageVisits(HttpClient httpClient)
		{
			_httpClient = httpClient;
		}

		public async Task<Response<IEnumerable<PageVisitsStat>>> GetPageVisitsStats(PostGetPageVisitsStats postModel)
		{
			try
			{
				var response = await _httpClient.PostAsJsonAsync("api/PageVisits/PostGetPageVisitsStats", postModel);
				if (response.IsSuccessStatusCode)
				{
					return await response.Content.ReadFromJsonAsync<Response<IEnumerable<PageVisitsStat>>>();
				}
				return Response<IEnumerable<PageVisitsStat>>.BadResult("Error al consultar las estadísticas", new List<PageVisitsStat>());
			}
			catch (Exception ex)
			{
				return Response<IEnumerable<PageVisitsStat>>.BadResult(ex.Message, new List<PageVisitsStat>());
			}
		}
	}
}
