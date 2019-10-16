using Microsoft.AspNetCore.Http;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace ApiWebServer.Core.Middleware
{
    public class WebInfoMiddleware
    {
        private static NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

        private readonly RequestDelegate _next;

        public WebInfoMiddleware( RequestDelegate next )
        {
            _next = next;
        }

        public async Task Invoke( HttpContext context )
        {
            // 요청 데이터
            var requestData = await FormatRequest( context.Request );
            _logger.Info( requestData );

            // 응답 데이터
            var responseData = await FormatResponse( context );
            _logger.Info( responseData );
        }

        private async Task<string> FormatRequest( HttpRequest request )
        {
            if ( request.ContentLength > 0 )
            {
                using ( var bodyReader = new StreamReader( request.Body ) )
                {
                    string bodyAsText = await bodyReader.ReadToEndAsync();
                    request.Body = new MemoryStream( Encoding.UTF8.GetBytes( bodyAsText ) );
                    return $"REQUEST {request.Path} {request.HttpContext.Connection.RemoteIpAddress} {bodyAsText.Length}";
                }
            }
            else
            {
                return $"REQUEST {request.Path} {request.HttpContext.Connection.RemoteIpAddress}";
            }
        }

        private async Task<string> FormatResponse( HttpContext context )
        {
            using ( var buffer = new MemoryStream() )
            {
                var stream = context.Response.Body;
                context.Response.Body = buffer;

                await _next( context );

                buffer.Seek( 0, SeekOrigin.Begin );
                var reader = new StreamReader( buffer );
                using ( var bufferReader = new StreamReader( buffer ) )
                {
                    string body = await bufferReader.ReadToEndAsync();

                    buffer.Seek( 0, SeekOrigin.Begin );
                    await buffer.CopyToAsync( stream );
                    context.Response.Body = stream;

                    return $"RESPONSE {context.Response.StatusCode} : {body.Length}";
                }
            }
        }
    }
}
