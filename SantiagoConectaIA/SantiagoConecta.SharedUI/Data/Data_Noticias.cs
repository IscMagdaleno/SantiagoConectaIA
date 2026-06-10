using EngramaCoreStandar.Results;
using Microsoft.AspNetCore.Components;
using Newtonsoft.Json;
using SantiagoConectaIA.Share.Objects.NoticiasModule;
using SantiagoConectaIA.Share.PostModels.NoticiasModule;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace SantiagoConecta.SharedUI.Data
{
    public class Data_Noticias
    {
        private readonly HttpClient _HttpClient;
        private readonly NavigationManager _navigator;

        public Data_Noticias(HttpClient httpClient, NavigationManager navigator)
        {
            _HttpClient = httpClient;
            _navigator = navigator;
            _HttpClient.DefaultRequestHeaders.Add("Accept", "text/plain");
            _HttpClient.DefaultRequestHeaders.Add("Accept", "application/json");
        }

        public async Task<Response<List<Noticia>>> PostGetAllNoticias(PostGetNoticias data)
        {
            var url = "/api/Noticias/PostGetNoticias";
            var request = new HttpRequestMessage(HttpMethod.Post, new Uri(_HttpClient.BaseAddress!, url));
            request.Content = new StringContent(JsonConvert.SerializeObject(data), Encoding.UTF8, "application/json");

            var respuesta = await _HttpClient.SendAsync(request);

            if (respuesta.StatusCode == HttpStatusCode.BadRequest)
            {
                return null;
            }

            var json = await respuesta.Content.ReadAsStringAsync();
            var resultado = JsonConvert.DeserializeObject<Response<List<Noticia>>>(json);
            return resultado;
        }
    }
}
