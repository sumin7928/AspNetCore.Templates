using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using ApiWebServer.Common;
using WebSharedLib.Core.NPLib;
using WebSharedLib.Core.Packet;

namespace ApiWebServer.Core.Controller
{
    public class NonSessionController<Request,Response> : ControllerBase
        where Request : PacketCommon, new()
        where Response : PacketCommon, new()
    {
        protected ILogger _logger;
        protected readonly IConfiguration _config;
        protected readonly IWebService<Request, Response> _webService;
        protected readonly IDBService _dbService;

        public NonSessionController( ILogger logger,
            IConfiguration config,
            IWebService<Request,Response> webService,
            IDBService dbService )
        {
            _logger = logger;
            _config = config;
            _webService = webService;
            _dbService = dbService;

            _webService.Logger = logger;
        }

        protected void WrapWebService( NPWebRequest requestBody )
        {
            _webService.WrapRequestData( HttpContext, requestBody );
        }
    }
}
