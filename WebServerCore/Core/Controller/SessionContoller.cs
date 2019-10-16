using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using ApiWebServer.Common;
using WebSharedLib.Core.NPLib;
using WebSharedLib.Core.Packet;
using WebSharedLib.Error;

namespace ApiWebServer.Core.Controller
{
    public class SessionContoller<Request, Response> : ControllerBase
        where Request : PacketCommon, new()
        where Response : PacketCommon, new()
    {
        protected readonly ILogger _logger;
        protected readonly IConfiguration _config;
        protected readonly IWebService<Request, Response> _webService;
        protected readonly IDBService _dbService;

        public SessionContoller(ILogger logger,
            IConfiguration config,
            IWebService<Request, Response> webService,
            IDBService dbService)
        {
            _logger = logger;
            _config = config;
            _webService = webService;
            _dbService = dbService;

            _webService.Logger = logger;
        }

        protected void WrapWebService(NPWebRequest requestBody)
        {
            _webService.WrapRequestDataWithSession(HttpContext, requestBody);
        }

        protected void CheckUrlFlow(string previousUrl)
        {
            if (_webService.WebSession == null)
            {
                return;
            }

            if (_webService.WebSession.PreviousUrl != previousUrl)
            {
                _webService.ErrorCode = ErrorCode.ERROR_URL_FLOW;
            }
        }

        protected void CheckUrlFlow(string[] previousUrls)
        {
            if (_webService.WebSession == null)
            {
                return;
            }

            bool isChecked = false;
            foreach (string url in previousUrls)
            {
                if (_webService.WebSession.PreviousUrl == url)
                {
                    isChecked = true;
                    break;
                }
            }

            if (isChecked == false)
            {
                _webService.ErrorCode = ErrorCode.ERROR_URL_FLOW;
            }
        }

    }
}
