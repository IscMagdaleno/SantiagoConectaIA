using EngramaCoreStandar.Results;
using Microsoft.AspNetCore.Components;
using Newtonsoft.Json;
using SantiagoConectaIA.Share.Objects.CiudadanoModule;
using SantiagoConectaIA.Share.PostModels.CiudadanoModule;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace SantiagoConecta.SharedUI.Data
{
    public class Data_Ciudadano
    {
        private readonly HttpClient _HttpClient;

        public Data_Ciudadano(HttpClient httpClient, NavigationManager navigator)
        {
            _HttpClient = httpClient;
            _HttpClient.DefaultRequestHeaders.Add("Accept", "text/plain");
            _HttpClient.DefaultRequestHeaders.Add("Accept", "application/json");
        }

        public Task<Response<Ciudadano>?> PostSaveCiudadano(PostSaveCiudadano data)
            => PostAsync("/api/Ciudadano/PostSaveCiudadano", data);

        public Task<Response<Ciudadano>?> PostLoginCiudadano(PostLoginCiudadano data)
            => PostAsync("/api/Ciudadano/PostLoginCiudadano", data);

        public Task<Response<Ciudadano>?> PostExternalLogin(PostExternalLoginCiudadano data)
            => PostAsync("/api/Ciudadano/PostExternalLogin", data);

        public Task<Response<string>?> PostEnviarCodigoWhatsApp(PostSendCodigoCiudadano data)
            => PostAsync<string, PostSendCodigoCiudadano>("/api/Ciudadano/PostEnviarCodigoWhatsApp", data);

        public async Task<Response<string>?> GetSocialAuthId(string proveedor)
        {
            var respuesta = await _HttpClient.GetAsync($"/api/Ciudadano/GetSocialAuthId?proveedor={System.Uri.EscapeDataString(proveedor)}");
            var json = await respuesta.Content.ReadAsStringAsync();
            if (string.IsNullOrWhiteSpace(json))
            {
                return null;
            }

            return JsonConvert.DeserializeObject<Response<string>>(json);
        }

        private Task<Response<Ciudadano>?> PostAsync<T>(string url, T data)
            => PostAsync<Ciudadano, T>(url, data);

        private async Task<Response<TResponse>?> PostAsync<TResponse, TRequest>(string url, TRequest data)
        {
            var request = new HttpRequestMessage(HttpMethod.Post, new Uri(_HttpClient.BaseAddress!, url));
            request.Content = new StringContent(JsonConvert.SerializeObject(data), Encoding.UTF8, "application/json");

            var respuesta = await _HttpClient.SendAsync(request);
            var json = await respuesta.Content.ReadAsStringAsync();
            if (string.IsNullOrWhiteSpace(json))
            {
                return null;
            }

            return JsonConvert.DeserializeObject<Response<TResponse>>(json);
        }
    }
}
