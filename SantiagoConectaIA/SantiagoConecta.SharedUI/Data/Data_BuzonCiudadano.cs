using EngramaCoreStandar.Results;

using Microsoft.AspNetCore.Components;

using Newtonsoft.Json;

using SantiagoConectaIA.Share.Objects.BuzonCiudadanoModule;
using SantiagoConectaIA.Share.PostModels.BuzonCiudadanoModule;

using System.Net;
using System.Text;

namespace SantiagoConecta.SharedUI.Data
{
	public class Data_BuzonCiudadano
	{
		private readonly HttpClient _HttpClient;

		public Data_BuzonCiudadano(HttpClient httpClient)
		{
			_HttpClient = httpClient;
			_HttpClient.DefaultRequestHeaders.Add("Accept", "application/json");
		}

		public async Task<Response<BuzonCiudadano>> PostSaveReporte(PostSaveBuzonCiudadano data)
		{
			var url = "/api/BuzonCiudadano/PostSaveReporte";
			var request = new HttpRequestMessage(HttpMethod.Post, new Uri(_HttpClient.BaseAddress!, url));
			request.Content = new StringContent(JsonConvert.SerializeObject(data), Encoding.UTF8, "application/json");

			var respuesta = await _HttpClient.SendAsync(request);

			var json = await respuesta.Content.ReadAsStringAsync();

			if (!respuesta.IsSuccessStatusCode)
			{
				return JsonConvert.DeserializeObject<Response<BuzonCiudadano>>(json)
					?? Response<BuzonCiudadano>.BadResult("Error al registrar el reporte.", new BuzonCiudadano());
			}

			var resultado = JsonConvert.DeserializeObject<Response<BuzonCiudadano>>(json);
			return resultado;
		}
	}
}
