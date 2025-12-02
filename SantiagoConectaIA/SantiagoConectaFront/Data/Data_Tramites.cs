using EngramaCoreStandar.Results;

using Microsoft.AspNetCore.Components;

using Newtonsoft.Json;

using SantiagoConectaIA.Share.Objects.TramitesModule;
using SantiagoConectaIA.Share.PostModels.TramitesModule;

using System.Net;
using System.Text;

namespace SantiagoConectaFront.Data
{
	public class Data_Tramites
	{
		private readonly HttpClient _HttpClient;
		private readonly NavigationManager _navigator;
		public Data_Tramites(HttpClient httpClient, NavigationManager navigator)
		{
			_HttpClient = httpClient;
			_navigator = navigator;
			_HttpClient.DefaultRequestHeaders.Add("Accept", "text/plain");
			_HttpClient.DefaultRequestHeaders.Add("Accept", "application/json");
		}
		public async Task<Response<List<Tramite>>> PostGetAllTramites(PostGetTramites data)
		{
			var url = "/api/Tramites/PostGetTramites";
			var request = new HttpRequestMessage(HttpMethod.Post, new Uri(_HttpClient.BaseAddress!, url));
			request.Content = new StringContent(JsonConvert.SerializeObject(data), Encoding.UTF8, "application/json");

			var respuesta = await _HttpClient.SendAsync(request);


			if (respuesta.StatusCode == HttpStatusCode.BadRequest)
			{
				return null;
			}

			var json = await respuesta.Content.ReadAsStringAsync();
			var resultado = JsonConvert.DeserializeObject<Response<List<Tramite>>>(json);
			return resultado;
		}
		public async Task<Response<List<Tramite>>> PostGetAllTramitesCard(PostGetTramites data)
		{
			var url = "/api/Tramites/PostGetTramitesCard";
			var request = new HttpRequestMessage(HttpMethod.Post, new Uri(_HttpClient.BaseAddress!, url));
			request.Content = new StringContent(JsonConvert.SerializeObject(data), Encoding.UTF8, "application/json");

			var respuesta = await _HttpClient.SendAsync(request);


			if (respuesta.StatusCode == HttpStatusCode.BadRequest)
			{
				return null;
			}

			var json = await respuesta.Content.ReadAsStringAsync();
			var resultado = JsonConvert.DeserializeObject<Response<List<Tramite>>>(json);
			return resultado;
		}

		public async Task<Response<Tramite>> PostGetTramiteDetalleAsync(PostGetTramiteDetalle data)
		{
			var url = "/api/Tramites/PostGetTramiteDetalle";
			var request = new HttpRequestMessage(HttpMethod.Post, new Uri(_HttpClient.BaseAddress!, url));
			request.Content = new StringContent(JsonConvert.SerializeObject(data), Encoding.UTF8, "application/json");

			var respuesta = await _HttpClient.SendAsync(request);

			var json = await respuesta.Content.ReadAsStringAsync();
			if (!respuesta.IsSuccessStatusCode)
			{
				// Devuelve el 'BadResult' de la API
				return JsonConvert.DeserializeObject<Response<Tramite>>(json);
			}

			var resultado = JsonConvert.DeserializeObject<Response<Tramite>>(json);
			return resultado;
		}
	}
}
