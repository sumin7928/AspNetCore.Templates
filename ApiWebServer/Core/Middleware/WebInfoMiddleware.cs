using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Internal;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace ApiServer.Core.Middleware
{
    public class WebInfoMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<WebInfoMiddleware> _logger;

        public WebInfoMiddleware( RequestDelegate next, ILogger<WebInfoMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task Invoke( HttpContext context )
        {
            // Reqeust Data
            var requestData = await FormatRequest(context.Request);
            _logger.LogInformation(requestData);

            // Response Data
            var responseData = await FormatResponse(context);
            _logger.LogInformation(responseData);
        }

        private async Task<string> FormatRequest(HttpRequest request)
        {
            if (request.ContentLength > 0)
            {
                using (var bodyReader = new StreamReader(request.Body))
                {
                    string bodyAsText = await bodyReader.ReadToEndAsync();
                    request.Body = new MemoryStream(Encoding.UTF8.GetBytes(bodyAsText));
                    return $"REQUEST {request.Scheme} {request.Method} {request.Host}{request.Path} {request.QueryString} {bodyAsText.Length} - {bodyAsText}";
                }
            }
            else
            {
                return $"REQUEST {request.Scheme} {request.Method} {request.Host}{request.Path} {request.QueryString}";
            }
        }

        private async Task<string> FormatResponse(HttpContext context)
        {
            using (var buffer = new MemoryStream())
            {
                var stream = context.Response.Body;
                context.Response.Body = buffer;

                await _next(context);

                buffer.Seek(0, SeekOrigin.Begin);
                using (var bufferReader = new StreamReader(buffer))
                {
                    string bodyAsText = await bufferReader.ReadToEndAsync();

                    buffer.Seek(0, SeekOrigin.Begin);
                    await buffer.CopyToAsync(stream);
                    context.Response.Body = stream;

                    if (bodyAsText.Length > 0)
                    {
                        return $"RESPONSE {context.Response.StatusCode} {bodyAsText.Length} - {bodyAsText}";
                    }
                    else
                    {
                        return $"RESPONSE {context.Response.StatusCode}";
                    }
                }
            }
        }
    }
}
