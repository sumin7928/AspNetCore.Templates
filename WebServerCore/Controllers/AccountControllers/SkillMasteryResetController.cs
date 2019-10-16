using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Data;
using ApiWebServer.Core;
using ApiWebServer.Core.Controller;
using ApiWebServer.Core.Swagger;
using ApiWebServer.Database.Utils;
using ApiWebServer.Logic;
using ApiWebServer.Models;
using WebSharedLib.Contents;
using WebSharedLib.Contents.Api;
using WebSharedLib.Core.NPLib;
using WebSharedLib.Entity;
using WebSharedLib.Error;

namespace ApiWebServer.Controllers.AccountControllers
{
    [Route( "api/Account/[controller]" )]
    [ApiController]
    public class SkillMasteryResetController : SessionContoller<ReqSkillMasteryReset, ResSkillMasteryReset>
    {
        public SkillMasteryResetController(
            ILogger<SkillMasteryResetController> logger,
            IConfiguration config,
            IWebService<ReqSkillMasteryReset, ResSkillMasteryReset> webService,
            IDBService dbService )
            : base( logger, config, webService, dbService )
        {
        }

        [HttpPost]
        [ApiExplorerSettings( GroupName = "client" )]
        [SwaggerExtend( "스킬 마스터리 초기화", typeof( SkillMasteryResetPacket ) )]
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

            DataSet dataSet = gameDB.USP_GS_GM_SKILL_MASTERY_RESET_R(webSession.TokenInfo.Pcid, reqData.Category);
            if (dataSet == null)
            {
                return _webService.End( ErrorCode.ERROR_DB, "USP_GS_GM_SKILL_MASTERY_RESET_R" );
            }

            DataSetWrapper dataSetWrapper = new DataSetWrapper( dataSet );

            List<SkillMastery> skillMasteryInfo = dataSetWrapper.GetObjectList<SkillMastery>( 0 );
            AccountGame accountGameInfo = dataSetWrapper.GetObject<AccountGame>( 1 );

            if( skillMasteryInfo.Count == 0 )
            {
                return _webService.End( ErrorCode.ERROR_NOT_FOUND_MASTERY_SKILL );
            }

            int resetPoint = GetResetMasteryPoint(skillMasteryInfo);

            // 비용 가져옴
            byte currencyType = (byte)Cache.CacheManager.PBTable.ConstantTable.Const.mastery_reset_cost_type;
            int currencyValue = Cache.CacheManager.PBTable.ConstantTable.Const.mastery_reset_cost_count;

            ConsumeReward consumeProcess = new ConsumeReward( webSession.TokenInfo.Pcid, gameDB, Common.Define.CONSUME_REWARD_TYPE.CONSUMEREWARD, false );
            consumeProcess.AddConsume(new GameRewardInfo(currencyType, 0, currencyValue) );
            consumeProcess.AddReward(new GameRewardInfo((byte)Common.Define.REWARD_TYPE.MASTERY_POINT, 0, resetPoint));

            ErrorCode consumeResult = consumeProcess.Run( ref accountGameInfo, true );

            if ( consumeResult != ErrorCode.SUCCESS )
            {
                return _webService.End( consumeResult );
            }

            if ( gameDB.USP_GS_GM_SKILL_MASTERY_RESET( webSession.TokenInfo.Pcid, reqData.Category, accountGameInfo) == false )
            {
                return _webService.End( ErrorCode.ERROR_DB, "USP_GS_GM_SKILL_MASTERY_RESET" );
            }

            resData.CostType = currencyType;
            resData.CostCount = currencyValue;
            resData.AccountCurrency = accountGameInfo;

            return _webService.End();
        }

        private int GetResetMasteryPoint(List<SkillMastery> nowRegisterdList)
        {
            int nowSkillPoint = 0;
            int registeredCnt = 0;

            foreach (var info in nowRegisterdList)
            {
                nowSkillPoint += (int)Math.Ceiling(((registeredCnt + 1) / (decimal)ApiWebServer.Cache.CacheManager.PBTable.ConstantTable.Const.mastery_cost));
                registeredCnt += 1;
            }

            return nowSkillPoint;
        }
    }
}
