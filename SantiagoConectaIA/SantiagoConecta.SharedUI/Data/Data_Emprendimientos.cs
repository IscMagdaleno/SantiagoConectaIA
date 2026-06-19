using EngramaCoreStandar.Results;
using Microsoft.AspNetCore.Components;
using Newtonsoft.Json;
using SantiagoConectaIA.Share.Objetos.EmpresasModulo;
using SantiagoConectaIA.Share.PostClass.EmpresasModulo;
using SantiagoConectaIA.Share.PostModels.EmpresasModulo;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace SantiagoConecta.SharedUI.Data
{
    public class Data_Emprendimientos
    {
        private readonly HttpClient _HttpClient;
        private readonly NavigationManager _navigator;

        public Data_Emprendimientos(HttpClient httpClient, NavigationManager navigator)
        {
            _HttpClient = httpClient;
            _navigator = navigator;
            _HttpClient.DefaultRequestHeaders.Add("Accept", "text/plain");
            _HttpClient.DefaultRequestHeaders.Add("Accept", "application/json");
        }

        private async Task<Response<T>> PostAsync<T>(string endpoint, object data)
        {
            var url = $"/api/Empresas/{endpoint}";
            var request = new HttpRequestMessage(HttpMethod.Post, new Uri(_HttpClient.BaseAddress!, url));
            request.Content = new StringContent(JsonConvert.SerializeObject(data), Encoding.UTF8, "application/json");

            var respuesta = await _HttpClient.SendAsync(request);

            if (respuesta.StatusCode == HttpStatusCode.BadRequest)
            {
                return null;
            }

            var json = await respuesta.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<Response<T>>(json);
        }

        public Task<Response<List<Empresa>>> PostGetEmpresas(PostGetEmpresas data) => PostAsync<List<Empresa>>("PostGetEmpresas", data);
        public Task<Response<List<CatalogoEmpresa>>> PostGetCatalogoEmpresa(PostGetCatalogoEmpresa data) => PostAsync<List<CatalogoEmpresa>>("PostGetCatalogoEmpresa", data);
        public Task<Response<List<EmpresaUbicacion>>> PostGetEmpresaUbicaciones(PostGetEmpresaUbicaciones data) => PostAsync<List<EmpresaUbicacion>>("PostGetEmpresaUbicaciones", data);
        public Task<Response<List<EmpresaRedSocial>>> PostGetEmpresaRedesSociales(PostGetEmpresaRedesSociales data) => PostAsync<List<EmpresaRedSocial>>("PostGetEmpresaRedesSociales", data);
        public Task<Response<List<CategoriaCatalogo>>> PostGetCategoriasPorEmpresa(PostGetCategoriasPorEmpresa data) => PostAsync<List<CategoriaCatalogo>>("PostGetCategoriasPorEmpresa", data);
        public Task<Response<List<ProductoServicio>>> PostGetProductosPorCategoria(PostGetProductosPorCategoria data) => PostAsync<List<ProductoServicio>>("PostGetProductosPorCategoria", data);
        public Task<Response<ConfiguracionVisual>> PostGetConfiguracionVisual(PostGetConfiguracionVisual data) => PostAsync<ConfiguracionVisual>("PostGetConfiguracionVisual", data);
    }
}
