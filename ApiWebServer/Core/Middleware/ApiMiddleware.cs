using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ApiWebServer.Core.Middleware
{
    public class ApiMiddleware
    {
        private readonly Action<Exception> _exception;
        private readonly RequestDelegate _next;
        private readonly ILogger<ApiMiddleware> _logger;
        private readonly IHostingEnvironment _env;
        private readonly IConfiguration _config;
        private readonly object widdlewareLock = new object();

        private DateTime _runDate = DateTime.Now.Date;
        private long _incrementNo = 0;

        public ApiMiddleware(Action<Exception> exception, RequestDelegate next, ILogger<ApiMiddleware> logger, IHostingEnvironment env, IConfiguration config)
        {
            _exception = exception;
            _next = next;
            _logger = logger;
            _env = env;
            _config = config;
        }

        public async Task Invoke(HttpContext context)
        {
            if (_runDate != DateTime.Now.Date)
            {
                lock (widdlewareLock) // api middleware singleton lock
                {
                    if (_runDate != DateTime.Now.Date)
                    {
                        _runDate = DateTime.Now.Date;
                        Interlocked.Exchange(ref _incrementNo, 0);
                    }
                }
            }

            long requestNo = Interlocked.Increment(ref _incrementNo);
            context.Items.Add("RequestNo", requestNo);

            try
            {
                _logger.LogInformation($"Api Start [{requestNo}] {context.Request.Method} {context.Request.Path}");
                await _next(context);
                _logger.LogInformation($"Api End [{requestNo}] {context.Request.Path} {context.Response.StatusCode}");
            }
            catch (Exception e)
            {
                _logger.LogError($"Res [{requestNo}] Exception Message - {e.Message}");
                _exception?.Invoke(e);
            }
        }
    }
}
