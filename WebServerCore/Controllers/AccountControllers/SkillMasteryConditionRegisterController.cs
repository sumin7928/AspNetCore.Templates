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
using Newtonsoft.Json;

namespace ApiWebServer.Controllers.AccountControllers
{
    [Route( "api/Account/[controller]" )]
    [ApiController]
    public class SkillMasteryConditionRegisterController : SessionContoller<ReqSkillMasteryConditionRegister, ResSkillMasteryConditionRegister>
    {
        public SkillMasteryConditionRegisterController(
            ILogger<SkillMasteryConditionRegisterController> logger,
            IConfiguration config,
            IWebService<ReqSkillMasteryConditionRegister, ResSkillMasteryConditionRegister> webService,
            IDBService dbService )
            : base( logger, config, webService, dbService )
        {
        }

        [HttpPost]
        [ApiExplorerSettings( GroupName = "client" )]
        [SwaggerExtend( "스킬 마스터리 발동 조건 등록", typeof( SkillMasteryConditionRegisterPacket ) )]
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

            if ( gameDB.USP_GS_GM_SKILL_MASTERY_CONDITION_REGISTER( webSession.TokenInfo.Pcid, reqData.Category, JsonConvert.SerializeObject(reqData.ConditionInfo)) == false )
            {
                return _webService.End( ErrorCode.ERROR_DB, "USP_GS_GM_SKILL_MASTERY_CONDITION_REGISTER");
            }
            return _webService.End();
        }
    }
}
