using EngramaCoreStandar.Results;

using Microsoft.AspNetCore.Components;

using Newtonsoft.Json;

using SantiagoConectaIA.Share.Objects.EventosModulo;
using SantiagoConectaIA.Share.PostClass.EventosModulo;

using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace SantiagoConecta.SharedUI.Data
{
    public class Data_Eventos
    {
        private readonly HttpClient _HttpClient;
        private readonly NavigationManager _navigator;

        public Data_Eventos(HttpClient httpClient, NavigationManager navigator)
        {
            _HttpClient = httpClient;
            _navigator = navigator;
            _HttpClient.DefaultRequestHeaders.Add("Accept", "text/plain");
            _HttpClient.DefaultRequestHeaders.Add("Accept", "application/json");
        }

        public async Task<Response<List<Evento>>> PostGetEventos(PostGetEventos data)
        {
            var url = "/api/Eventos/PostGetEventos";
            var request = new HttpRequestMessage(HttpMethod.Post, new Uri(_HttpClient.BaseAddress!, url));
            request.Content = new StringContent(JsonConvert.SerializeObject(data), Encoding.UTF8, "application/json");

            var respuesta = await _HttpClient.SendAsync(request);
            if (respuesta.StatusCode == HttpStatusCode.BadRequest) return null;

            var json = await respuesta.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<Response<List<Evento>>>(json);
        }

        public async Task<Response<List<CategoriaEvento>>> PostGetCategoriaEventos()
        {
            var url = "/api/Eventos/PostGetCategoriaEventos";
            var request = new HttpRequestMessage(HttpMethod.Post, new Uri(_HttpClient.BaseAddress!, url));
            request.Content = new StringContent(JsonConvert.SerializeObject(new PostGetCategoriaEventos()), Encoding.UTF8, "application/json");

            var respuesta = await _HttpClient.SendAsync(request);
            if (respuesta.StatusCode == HttpStatusCode.BadRequest) return null;

            var json = await respuesta.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<Response<List<CategoriaEvento>>>(json);
        }

        public async Task<Response<EventoDetalle>> PostGetEventoDetalle(int idEvento)
        {
            var url = "/api/Eventos/PostGetEventoDetalle";
            var request = new HttpRequestMessage(HttpMethod.Post, new Uri(_HttpClient.BaseAddress!, url));
            request.Content = new StringContent(JsonConvert.SerializeObject(new PostGetEventoDetalle { iIdEvento = idEvento }), Encoding.UTF8, "application/json");

            var respuesta = await _HttpClient.SendAsync(request);
            if (respuesta.StatusCode == HttpStatusCode.BadRequest) return null;

            var json = await respuesta.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<Response<EventoDetalle>>(json);
        }

        public async Task<Response<List<ImagenRegistro>>> PostGetImagenesRegistro(PostGetImagenesRegistro data)
        {
            var url = "/api/Eventos/PostGetImagenesRegistro";
            var request = new HttpRequestMessage(HttpMethod.Post, new Uri(_HttpClient.BaseAddress!, url));
            request.Content = new StringContent(JsonConvert.SerializeObject(data), Encoding.UTF8, "application/json");

            var respuesta = await _HttpClient.SendAsync(request);
            if (respuesta.StatusCode == HttpStatusCode.BadRequest) return null;

            var json = await respuesta.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<Response<List<ImagenRegistro>>>(json);
        }

        public async Task<Response<List<SucursalEvento>>> PostGetEventosSucursales(int idEvento)
        {
            var url = "/api/Eventos/PostGetEventosSucursales";
            var request = new HttpRequestMessage(HttpMethod.Post, new Uri(_HttpClient.BaseAddress!, url));
            request.Content = new StringContent(JsonConvert.SerializeObject(new PostGetEventosSucursales { iIdEvento = idEvento, bActivo = true }), Encoding.UTF8, "application/json");

            var respuesta = await _HttpClient.SendAsync(request);
            if (respuesta.StatusCode == HttpStatusCode.BadRequest) return null;

            var json = await respuesta.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<Response<List<SucursalEvento>>>(json);
        }
    }
}
