using EngramaCoreStandar.Results;
using Microsoft.AspNetCore.Components;
using Newtonsoft.Json;
using SantiagoConectaIA.Share.Objects.PublicacionCiudadanoModule;
using SantiagoConectaIA.Share.PostModels.PublicacionCiudadanoModule;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace SantiagoConecta.SharedUI.Data
{
    public class Data_PublicacionesCiudadano
    {
        private readonly HttpClient _HttpClient;

        public Data_PublicacionesCiudadano(HttpClient httpClient, NavigationManager navigator)
        {
            _HttpClient = httpClient;
            _HttpClient.DefaultRequestHeaders.Add("Accept", "text/plain");
            _HttpClient.DefaultRequestHeaders.Add("Accept", "application/json");
        }

        public Task<Response<List<PublicacionCiudadano>>?> PostGetPublicaciones(PostGetPublicacionesCiudadano data)
            => PostAsync<List<PublicacionCiudadano>, PostGetPublicacionesCiudadano>("/api/PublicacionesCiudadano/PostGetPublicacionesCiudadano", data, token: null);

        public Task<Response<PublicacionCiudadano>?> PostSavePublicacion(PostSavePublicacionCiudadano data, string token)
            => PostAsync<PublicacionCiudadano, PostSavePublicacionCiudadano>("/api/PublicacionesCiudadano/PostSavePublicacionCiudadano", data, token);

        public Task<Response<List<PublicacionCiudadano>>?> PostGetMisPublicaciones(PostGetMisPublicacionesCiudadano data, string token)
            => PostAsync<List<PublicacionCiudadano>, PostGetMisPublicacionesCiudadano>("/api/PublicacionesCiudadano/PostGetMisPublicacionesCiudadano", data, token);

        public Task<Response<string>?> PostDeletePublicacion(PostDeletePublicacionCiudadano data, string token)
            => PostAsync<string, PostDeletePublicacionCiudadano>("/api/PublicacionesCiudadano/PostDeletePublicacionCiudadano", data, token);

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
                    Message = "Tu sesión ha expirado o no tienes permisos. Vuelve a iniciar sesión en Únete."
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
