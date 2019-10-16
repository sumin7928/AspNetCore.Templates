using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Data;
using ApiWebServer.Cache;
using ApiWebServer.Core;
using ApiWebServer.Core.Controller;
using ApiWebServer.Core.Swagger;
using ApiWebServer.Database.Utils;
using ApiWebServer.Models;
using WebSharedLib.Contents;
using WebSharedLib.Contents.Api;
using WebSharedLib.Core.NPLib;
using WebSharedLib.Entity;
using WebSharedLib.Error;
using ApiWebServer.Common.Define;

namespace ApiWebServer.Controllers.ClientTestContollers
{
    [Route( "api/ClientTest/[controller]" )]
    [ApiController]
    public class RewardTestController : SessionContoller<ReqRewardTest, ResRewardTest>
    {
        public RewardTestController(
            ILogger<RewardTestController> logger,
            IConfiguration config,
            IWebService<ReqRewardTest, ResRewardTest> webService,
            IDBService dbService )
            : base( logger, config, webService, dbService )
        {
        }

        [HttpPost]
        [ApiExplorerSettings( GroupName = "client" )]
        [SwaggerExtend( "보상 테스트", typeof( RewardTestPacket ) )]
        public NPWebResponse Contoller( [FromBody] NPWebRequest requestBody )
        {
            WrapWebService( requestBody );
            if ( _webService.ErrorCode != ErrorCode.SUCCESS )
            {
                return _webService.End( _webService.ErrorCode );
            }

            // Business
            var webSession = _webService.WebSession;
            var reqData = _webService.WebPacket.ReqData;
            var resData = _webService.WebPacket.ResData;
            var gameDB = _dbService.CreateGameDB( _webService.RequestNo, webSession.DBNo );
            var postDB = _dbService.CreatePostDB(_webService.RequestNo, webSession.DBNo);

            if (reqData.TestType == 1 && reqData.GameRewardInfo != null)
            {
                PostInsert postInsert = new PostInsert(webSession.PubId, reqData.GameRewardInfo);

                if (postDB.USP_GS_PO_POST_SEND(webSession.TokenInfo.Pcid, webSession.UserName, -1, "admin", postInsert, (byte)POST_ADD_TYPE.ONE_BY_ONE) == false)
                {
                    return _webService.End(ErrorCode.ERROR_DB, "USP_GS_PO_POST_SEND");
                }
            }

            return _webService.End();
        }
    }
}
