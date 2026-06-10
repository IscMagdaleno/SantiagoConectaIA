using Microsoft.AspNetCore.Http;
using SantiagoConectaIA.API.EngramaLevels.Domain.Interfaces;
using SantiagoConectaIA.Share.PostModels.LogsModulo;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace SantiagoConectaIA.API.Middleware
{
    public class ApiLoggingMiddleware
    {
        private readonly RequestDelegate _next;

        public ApiLoggingMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context, ILogsDomain logsDomain)
        {
            var request = context.Request;
            var requestTime = DateTime.UtcNow;
            var requestBodyContent = await ReadRequestBody(request);

            var originalBodyStream = context.Response.Body;
            using var responseBody = new MemoryStream();
            context.Response.Body = responseBody;

            try
            {
                await _next(context);
            }
            finally
            {
                var responseTime = DateTime.UtcNow;
                var duration = (int)(responseTime - requestTime).TotalMilliseconds;

                responseBody.Seek(0, SeekOrigin.Begin);
                var responseBodyContent = await new StreamReader(responseBody).ReadToEndAsync();
                responseBody.Seek(0, SeekOrigin.Begin);

                await responseBody.CopyToAsync(originalBodyStream);
                context.Response.Body = originalBodyStream;

                var log = new PostSaveApiCallLog
                {
                    vchEndpoint = request.Path,
                    vchRequestMethod = request.Method,
                    dtRequestTimestamp = requestTime,
                    nvchRequestBody = requestBodyContent,
                    dtResponseTimestamp = responseTime,
                    nvchResponseBody = responseBodyContent,
                    bIsSuccess = context.Response.StatusCode >= 200 && context.Response.StatusCode < 300,
                    iDurationMs = duration,
                    vchHost = request.Host.ToString()
                };

                // Fire and forget logging? Or await?
                // Given the user requirement is strict logging, we should probably await to ensure it's saved.
                // But we must capture logging errors to not break the response if logging fails (handled in Domain).
                await logsDomain.SaveApiCallLog(log);
            }
        }

        private async Task<string> ReadRequestBody(HttpRequest request)
        {
            request.EnableBuffering();
            using var reader = new StreamReader(request.Body, Encoding.UTF8, true, 1024, true);
            var body = await reader.ReadToEndAsync();
            request.Body.Position = 0;
            return body;
        }
    }
}
