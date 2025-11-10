using Microsoft.AspNetCore.Components;

namespace SantiagoConectaFront.Data
{
	public class Data_DB
	{
		private readonly HttpClient _HttpClient;
		private readonly NavigationManager _navigator;
		public Data_DB(HttpClient httpClient, NavigationManager navigator)
		{
			_HttpClient = httpClient;
			_navigator = navigator;
			_HttpClient.DefaultRequestHeaders.Add("Accept", "text/plain");
			_HttpClient.DefaultRequestHeaders.Add("Accept", "application/json");
		}


	}
}
