using EngramaCoreStandar.Results;
using Microsoft.AspNetCore.Components;
using Newtonsoft.Json;
using SantiagoConectaIA.Share.Objects.OpinionModule;
using SantiagoConectaIA.Share.PostModels.OpinionModule;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace SantiagoConecta.SharedUI.Data
{
    public class Data_Opinion
    {
        private readonly HttpClient _HttpClient;

        public Data_Opinion(HttpClient httpClient, NavigationManager navigator)
        {
            _HttpClient = httpClient;
            _HttpClient.DefaultRequestHeaders.Add("Accept", "text/plain");
            _HttpClient.DefaultRequestHeaders.Add("Accept", "application/json");
        }

        public Task<Response<List<Opinion>>?> PostGetOpiniones(PostGetOpiniones data)
            => PostAsync<List<Opinion>, PostGetOpiniones>("/api/Opinion/PostGetOpiniones", data, token: null);

        public Task<Response<Opinion>?> PostSaveOpinion(PostSaveOpinion data, string token)
            => PostAsync<Opinion, PostSaveOpinion>("/api/Opinion/PostSaveOpinion", data, token);

        private async Task<Response<TResponse>?> PostAsync<TResponse, TRequest>(string url, TRequest data, string? token)
        {
            var request = new HttpRequestMessage(HttpMethod.Post, new Uri(_HttpClient.BaseAddress!, url));
            request.Content = new StringContent(JsonConvert.SerializeObject(data), Encoding.UTF8, "application/json");

            if (!string.IsNullOrWhiteSpace(token))
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }

            var respuesta = await _HttpClient.SendAsync(request);
            var json = await respuesta.Content.ReadAsStringAsync();

            if (respuesta.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                return new Response<TResponse>
                {
                    IsSuccess = false,
                    Message = "Sesión inválida. Entra de nuevo en Únete."
                };
            }

            if (string.IsNullOrWhiteSpace(json))
            {
                return null;
            }

            return JsonConvert.DeserializeObject<Response<TResponse>>(json);
        }
    }
}
