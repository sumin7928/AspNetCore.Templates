using System.Collections.Generic;
using System.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using ApiWebServer.Cache;
using ApiWebServer.Core;
using ApiWebServer.Core.Controller;
using ApiWebServer.Core.Swagger;
using ApiWebServer.Database.Utils;
using ApiWebServer.Logic;
using ApiWebServer.Models;
using ApiWebServer.Common.Define;
using WebSharedLib.Contents;
using WebSharedLib.Contents.Api;
using WebSharedLib.Core.NPLib;
using WebSharedLib.Entity;
using WebSharedLib.Error;

namespace ApiWebServer.Controllers.AccountControllers
{
    [Route( "api/Account/[controller]" )]
    [ApiController]
    public class SkillMasteryRegisterController : SessionContoller<ReqSkillMasteryRegister, ResSkillMasteryRegister>
    {
        public SkillMasteryRegisterController(
            ILogger<SkillMasteryRegisterController> logger,
            IConfiguration config,
            IWebService<ReqSkillMasteryRegister, ResSkillMasteryRegister> webService,
            IDBService dbService )
            : base( logger, config, webService, dbService )
        {
        }

        [HttpPost]
        [ApiExplorerSettings( GroupName = "client" )]
        [SwaggerExtend( "스킬 마스터리 등록", typeof( SkillMasteryRegisterPacket ) )]
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

            DataSet dataSet = gameDB.USP_GS_GM_SKILL_MASTERY_REGISTER_R(webSession.TokenInfo.Pcid, reqData.Category);
            if (dataSet == null)
            {
                return _webService.End( ErrorCode.ERROR_DB, "USP_GS_GM_SKILL_MASTERY_REGISTER_R" );
            }

            DataSetWrapper dataSetWrapper = new DataSetWrapper( dataSet );

            List<SkillMastery> skillMasteryInfo = dataSetWrapper.GetObjectList<SkillMastery>( 0 );
            AccountGame accountGameInfo = dataSetWrapper.GetObject<AccountGame>( 1 );

            ErrorCode registerResult = CacheManager.PBTable.ManagerTable.RegisterMasterySkill( reqData, accountGameInfo, skillMasteryInfo );
            if ( registerResult != ErrorCode.SUCCESS )
            {
                return _webService.End( registerResult );
            }

            ConsumeReward consumeProcess = new ConsumeReward(webSession.TokenInfo.Pcid, gameDB, Common.Define.CONSUME_REWARD_TYPE.CONSUME, false);
            consumeProcess.AddConsume(new GameRewardInfo((byte)REWARD_TYPE.MASTERY_POINT, 0, reqData.UseSkillPoint));
            ErrorCode consumeResult = consumeProcess.Run(ref accountGameInfo, true);

            if (consumeResult != ErrorCode.SUCCESS)
            {
                return _webService.End(consumeResult);
            }

            if ( gameDB.USP_GS_GM_SKILL_MASTERY_REGISTER( webSession.TokenInfo.Pcid, skillMasteryInfo, accountGameInfo) == false )
            {
                return _webService.End( ErrorCode.ERROR_DB, "USP_GS_GM_SKILL_MASTERY_REGISTER" );
            }

            resData.MasteryList = skillMasteryInfo;
            resData.AccountCurrency = accountGameInfo;
            return _webService.End();
        }
    }
}
