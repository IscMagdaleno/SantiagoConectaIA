using EngramaCoreStandar.Results;
using Microsoft.AspNetCore.Components;
using Newtonsoft.Json;
using SantiagoConectaIA.Share.Objetos.InformacionLocalModulo;
using SantiagoConectaIA.Share.PostClass.InformacionLocalModulo;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace SantiagoConecta.SharedUI.Data
{
    public class Data_InformacionLocal
    {
        private readonly HttpClient _HttpClient;
        private readonly NavigationManager _navigator;

        public Data_InformacionLocal(HttpClient httpClient, NavigationManager navigator)
        {
            _HttpClient = httpClient;
            _navigator = navigator;
            _HttpClient.DefaultRequestHeaders.Add("Accept", "text/plain");
            _HttpClient.DefaultRequestHeaders.Add("Accept", "application/json");
        }

        public async Task<Response<List<InformacionLocal>>> PostGetInformacionLocal(PostGetInformacionLocal data)
        {
            var url = "/api/InformacionLocal/PostGetInformacionLocal";
            var request = new HttpRequestMessage(HttpMethod.Post, new Uri(_HttpClient.BaseAddress!, url));
            request.Content = new StringContent(JsonConvert.SerializeObject(data), Encoding.UTF8, "application/json");

            var respuesta = await _HttpClient.SendAsync(request);
            if (respuesta.StatusCode == HttpStatusCode.BadRequest) return null;

            var json = await respuesta.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<Response<List<InformacionLocal>>>(json);
        }
    }
}
