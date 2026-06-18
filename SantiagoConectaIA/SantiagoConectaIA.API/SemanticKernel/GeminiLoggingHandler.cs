using System.Net.Http;
using System.Net.Http.Json;
using System.Text;

namespace SantiagoConectaIA.API.SemanticKernel
{
	public class GeminiLoggingHandler : DelegatingHandler
	{
		private readonly ILogger<GeminiLoggingHandler> _logger;

		public GeminiLoggingHandler(ILogger<GeminiLoggingHandler> logger)
		{
			_logger = logger;
		}

		protected override async Task<HttpResponseMessage> SendAsync(
			HttpRequestMessage request, CancellationToken cancellationToken)
		{
			var requestId = Guid.NewGuid().ToString("N")[..8];

			// Log request
			if (request.Content != null)
			{
				var requestBody = await request.Content.ReadAsStringAsync(cancellationToken);
				_logger.LogInformation("[Gemini REQ {Id}] URL: {Url} | Body length: {Len}",
					requestId, request.RequestUri, requestBody.Length);

				if (requestBody.Length > 500)
				{
					_logger.LogInformation("[Gemini REQ {Id}] Body (first 500): {Body}",
						requestId, requestBody[..500]);
					_logger.LogInformation("[Gemini REQ {Id}] Body (last 500): {Body}",
						requestId, requestBody[^500..]);
				}
				else
				{
					_logger.LogInformation("[Gemini REQ {Id}] Body: {Body}",
						requestId, requestBody);
				}
			}

			var response = await base.SendAsync(request, cancellationToken);

			// Log response
			var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);
			if (!response.IsSuccessStatusCode)
			{
				_logger.LogError("[Gemini RESP {Id}] Status: {Status} | Body: {Body}",
					requestId, (int)response.StatusCode, responseBody);
			}
			else
			{
				_logger.LogInformation("[Gemini RESP {Id}] Status: {Status} | Body length: {Len}",
					requestId, (int)response.StatusCode, responseBody.Length);

				if (responseBody.Length > 500)
				{
					_logger.LogInformation("[Gemini RESP {Id}] Body (last 500): {Body}",
						requestId, responseBody[^500..]);
				}
			}

			return response;
		}
	}
}
