using EngramaCoreStandar.Results;

using Microsoft.AspNetCore.Components;

using Newtonsoft.Json;

using SantiagoConectaIA.Share.Objects.AnalyticsModule;
using SantiagoConectaIA.Share.PostModels.AnalyticsModule;

using System.Net;
using System.Text;

namespace SantiagoConecta.SharedUI.Data
{
    public class Data_Analytics
    {
        private readonly HttpClient _HttpClient;

        public Data_Analytics(HttpClient httpClient)
        {
            _HttpClient = httpClient;
            _HttpClient.DefaultRequestHeaders.Add("Accept", "application/json");
        }

        public async Task<Response<PageVisitSaveResult>> PostSavePageVisit(PostSavePageVisit data)
        {
            var url = "/api/Analytics/PostSavePageVisit";
            var request = new HttpRequestMessage(HttpMethod.Post, new Uri(_HttpClient.BaseAddress!, url));
            request.Content = new StringContent(JsonConvert.SerializeObject(data), Encoding.UTF8, "application/json");

            var respuesta = await _HttpClient.SendAsync(request);
            var json = await respuesta.Content.ReadAsStringAsync();

            if (!respuesta.IsSuccessStatusCode)
            {
                return JsonConvert.DeserializeObject<Response<PageVisitSaveResult>>(json)
                    ?? Response<PageVisitSaveResult>.BadResult("Error al registrar la visita.", new PageVisitSaveResult());
            }

            var resultado = JsonConvert.DeserializeObject<Response<PageVisitSaveResult>>(json);
            return resultado;
        }

        public async Task<IEnumerable<PageVisitSummary>> PostGetPageVisitsSummary(DateTime? startDate = null, DateTime? endDate = null)
        {
            var url = "/api/Analytics/PostGetPageVisitsSummary";
            var model = new { dtStartDate = startDate, dtEndDate = endDate };
            var request = new HttpRequestMessage(HttpMethod.Post, new Uri(_HttpClient.BaseAddress!, url));
            request.Content = new StringContent(JsonConvert.SerializeObject(model), Encoding.UTF8, "application/json");

            var respuesta = await _HttpClient.SendAsync(request);
            var json = await respuesta.Content.ReadAsStringAsync();

            if (!respuesta.IsSuccessStatusCode)
            {
                return new List<PageVisitSummary> { new() { bResult = false, vchMessage = "Error al obtener resumen." } };
            }

            return JsonConvert.DeserializeObject<IEnumerable<PageVisitSummary>>(json)
                ?? new List<PageVisitSummary>();
        }

        public async Task<IEnumerable<PageVisitByPage>> PostGetPageVisitsByPage(DateTime? startDate = null, DateTime? endDate = null)
        {
            var url = "/api/Analytics/PostGetPageVisitsByPage";
            var model = new { dtStartDate = startDate, dtEndDate = endDate };
            var request = new HttpRequestMessage(HttpMethod.Post, new Uri(_HttpClient.BaseAddress!, url));
            request.Content = new StringContent(JsonConvert.SerializeObject(model), Encoding.UTF8, "application/json");

            var respuesta = await _HttpClient.SendAsync(request);
            var json = await respuesta.Content.ReadAsStringAsync();

            if (!respuesta.IsSuccessStatusCode)
            {
                return new List<PageVisitByPage> { new() { bResult = false, vchMessage = "Error al obtener visitas por página." } };
            }

            return JsonConvert.DeserializeObject<IEnumerable<PageVisitByPage>>(json)
                ?? new List<PageVisitByPage>();
        }

        public async Task<IEnumerable<DailyTraffic>> PostGetDailyTraffic(DateTime? startDate = null, DateTime? endDate = null)
        {
            var url = "/api/Analytics/PostGetDailyTraffic";
            var model = new { dtStartDate = startDate, dtEndDate = endDate };
            var request = new HttpRequestMessage(HttpMethod.Post, new Uri(_HttpClient.BaseAddress!, url));
            request.Content = new StringContent(JsonConvert.SerializeObject(model), Encoding.UTF8, "application/json");

            var respuesta = await _HttpClient.SendAsync(request);
            var json = await respuesta.Content.ReadAsStringAsync();

            if (!respuesta.IsSuccessStatusCode)
            {
                return new List<DailyTraffic> { new() { bResult = false, vchMessage = "Error al obtener tráfico diario." } };
            }

            return JsonConvert.DeserializeObject<IEnumerable<DailyTraffic>>(json)
                ?? new List<DailyTraffic>();
        }

        public async Task<IEnumerable<PageVisit>> PostGetRecentVisits(int topRows = 100)
        {
            var url = "/api/Analytics/PostGetRecentVisits";
            var model = new { iTopRows = topRows };
            var request = new HttpRequestMessage(HttpMethod.Post, new Uri(_HttpClient.BaseAddress!, url));
            request.Content = new StringContent(JsonConvert.SerializeObject(model), Encoding.UTF8, "application/json");

            var respuesta = await _HttpClient.SendAsync(request);
            var json = await respuesta.Content.ReadAsStringAsync();

            if (!respuesta.IsSuccessStatusCode)
            {
                return new List<PageVisit> { new() { bIsUniqueVisitor = false, vchPageUrl = "Error" } };
            }

            return JsonConvert.DeserializeObject<IEnumerable<PageVisit>>(json)
                ?? new List<PageVisit>();
        }
    }
}
