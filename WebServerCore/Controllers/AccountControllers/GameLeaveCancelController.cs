//using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using ApiWebServer.Core;
using ApiWebServer.Core.Controller;
using ApiWebServer.Core.Swagger;
using WebSharedLib.Contents;
using WebSharedLib.Contents.Api;
using WebSharedLib.Core.NPLib;
using WebSharedLib.Error;

namespace ApiWebServer.Controllers.AccountControllers
{
    [Route("api/Account/[controller]")]
    [ApiController]
    public class GameLeaveCancelController : SessionContoller<ReqGameLeaveCancel, ResGameLeaveCancel>
    {
        public GameLeaveCancelController(
            ILogger<GameLeaveCancelController> logger,
            IConfiguration config, 
            IWebService<ReqGameLeaveCancel, ResGameLeaveCancel> webService, 
            IDBService dbService )
            : base( logger, config, webService, dbService )
        {
        }

        [HttpPost]
        [ApiExplorerSettings( GroupName = "client" )]
        [SwaggerExtend( "게임 탈퇴 요청 취소", typeof( GameLeaveCancelPacket ) )]
        public NPWebResponse Controller([FromBody] NPWebRequest requestBody )
        {
            WrapWebService( requestBody );
            if ( _webService.ErrorCode != ErrorCode.SUCCESS )
            {
                return _webService.End( _webService.ErrorCode );
            }

            // Business
            var webSession = _webService.WebSession;
            var accountDB = _dbService.CreateAccountDB( _webService.RequestNo );

            if ( accountDB.GameLeaveCancel(webSession.PubType, webSession.PubId) == false)
            {
                return _webService.End( ErrorCode.ERROR_DB, "GameLeaveCancel" );
            }

            return _webService.End();
        }
    }
}
