using EngramaCoreStandar.Results;
using Microsoft.AspNetCore.Components;
using Newtonsoft.Json;
using SantiagoConectaIA.Share.Objects.FeedModule;
using SantiagoConectaIA.Share.PostModels.FeedModule;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace SantiagoConecta.SharedUI.Data
{
    public class Data_Feed
    {
        private readonly HttpClient _HttpClient;

        public Data_Feed(HttpClient httpClient, NavigationManager navigator)
        {
            _HttpClient = httpClient;
            _HttpClient.DefaultRequestHeaders.Add("Accept", "text/plain");
            _HttpClient.DefaultRequestHeaders.Add("Accept", "application/json");
        }

        public async Task<Response<List<FeedCard>>> PostGetFeed(PostGetFeed data)
        {
            var url = "/api/Feed/PostGetFeed";
            var request = new HttpRequestMessage(HttpMethod.Post, new Uri(_HttpClient.BaseAddress!, url));
            request.Content = new StringContent(JsonConvert.SerializeObject(data), Encoding.UTF8, "application/json");

            var respuesta = await _HttpClient.SendAsync(request);
            if (respuesta.StatusCode == HttpStatusCode.BadRequest)
            {
                return null;
            }

            var json = await respuesta.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<Response<List<FeedCard>>>(json);
        }

        public async Task<Response<FeedSearchResult>> PostSearchFeed(PostSearchFeed data)
        {
            var url = "/api/Feed/PostSearchFeed";
            var request = new HttpRequestMessage(HttpMethod.Post, new Uri(_HttpClient.BaseAddress!, url));
            request.Content = new StringContent(JsonConvert.SerializeObject(data), Encoding.UTF8, "application/json");

            var respuesta = await _HttpClient.SendAsync(request);
            if (respuesta.StatusCode == HttpStatusCode.BadRequest)
            {
                return null;
            }

            var json = await respuesta.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<Response<FeedSearchResult>>(json);
        }
    }
}
