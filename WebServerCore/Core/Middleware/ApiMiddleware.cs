using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ApiWebServer.Common.Define;

namespace ApiWebServer.Core.Middleware
{
    public class ApiMiddleware
    {
        private static NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

        private readonly RequestDelegate _next;
        private readonly IHostingEnvironment _env;
        private readonly IConfiguration _config;
        private readonly IHttpClientFactory _httpClientFactory;

        private DateTime _runDate = DateTime.Now.Date;
        private long _incrementNo = 0;

        public ApiMiddleware( RequestDelegate next, IHostingEnvironment env, IConfiguration config, IHttpClientFactory httpClientFactory )
        {
            _env = env;
            _next = next;
            _config = config;
            _httpClientFactory = httpClientFactory;
        }

        public async Task Invoke( HttpContext context )
        {
            // 점검 체크
            if ( IsInspection( context ) == true )
            {
                string message = _config[ "Message" ];
                context.Response.StatusCode = StatusCodes.Status503ServiceUnavailable;
                context.Response.Body.Write( Encoding.UTF8.GetBytes( message ), 0, message.Length );
                return;
            }

            if (_runDate != DateTime.Now.Date) // thread not safe 허용
            {
                _runDate = DateTime.Now.Date;
                Interlocked.Exchange(ref _incrementNo, 0);
            }

            long requestNo = Interlocked.Increment( ref _incrementNo );
            string uriPath = context.Request.Path;
            string method = context.Request.Method;
            context.Request.Headers.Add( WEB_HEADER_PROPERTIES.REQUEST_NO.ToString(), requestNo.ToString() );

            try
            {
                await _next( context );

                object alarmMessage = context.Items[ "alarm" ];
                if( alarmMessage != null )
                {
                    _logger.Error( "[Alarm] ERROR [{0}] - {1} ", requestNo, alarmMessage );

                    bool isUse = bool.Parse( _config.GetSection( "AlarmServer" )[ "UseFlag" ] );
                    if ( isUse )
                    {
                        await SendToAlarmServer( ( string )alarmMessage );
                    }
                }
            }
            catch ( Exception e )
            {
                _logger.Error( e, "[Alarm] EXCEPTION [{0}] - {1} ", requestNo, e.Message );

                string responseMessage = "Exception during processing request";
                context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                context.Response.Body.Write( Encoding.UTF8.GetBytes( responseMessage ), 0, responseMessage.Length );

                StringBuilder sb = new StringBuilder();
                sb.AppendLine( "Controller Exception : Source:" + e.Source );
                sb.AppendLine( e.Message );
                sb.AppendLine( e.StackTrace.Substring( 0, 512 ) );

                bool isUse = bool.Parse( _config.GetSection( "AlarmServer" )[ "UseFlag" ] );
                if( isUse )
                {
                    await SendToAlarmServer( sb.ToString() );
                }
            }
        }

        private bool IsInspection ( HttpContext context )
        {
            if ( bool.Parse( _config.GetSection( "ServerInspection" )[ "Status" ] ) == false )
            {
                return false;
            }

            // 점검 예외 처리
            if ( context.Request.Path.Value.Contains( "api/admin" ) == true )
            {
                return false;
            }
            else
            {
                // 허용된 유저 체크
                string user = context.Request.Headers[ "pubId" ];
                string allowedUsers = _config.GetSection( "ServerInspection" )[ "AllowedUsers" ];
                if ( allowedUsers != null && allowedUsers.Contains( user ) == true )
                {
                    return false;
                }

                return true;
            }
        }

        private async Task SendToAlarmServer( string contents )
        {
            string url = _config.GetSection( "AlarmServer" )[ "Url" ];
            string projectName = _config.GetSection( "AlarmServer" )[ "ProjectName" ];
            string groupName = _config.GetSection( "AlarmServer" )[ "GroupName" ];

            Dictionary<string, string> paramsData = new Dictionary<string, string>
                    {
                        { "ProjectName", projectName },
                        { "GroupName", groupName },
                        { "MessageType", _env.EnvironmentName },
                        { "Contents", contents }
                    };

            HttpRequestMessage request = new HttpRequestMessage( HttpMethod.Post, url + "/Error" )
            {
                Content = new FormUrlEncodedContent( paramsData )
            };

            HttpClient httpClient = _httpClientFactory.CreateClient();
            HttpResponseMessage response = await httpClient.SendAsync( request );
            if ( response.IsSuccessStatusCode == false )
            {
                _logger.Warn( "Failed to sending message to alarm server - {0}", contents );
            }
        }
    }
}
