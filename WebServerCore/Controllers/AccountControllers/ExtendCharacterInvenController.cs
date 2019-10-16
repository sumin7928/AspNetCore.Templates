using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
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
    public class ExtendCharacterInvenController : SessionContoller<ReqExtendCharacterInven, ResExtendCharacterInven>
    {
        public ExtendCharacterInvenController(
            ILogger<ExtendCharacterInvenController> logger,
            IConfiguration config,
            IWebService<ReqExtendCharacterInven, ResExtendCharacterInven> webService,
            IDBService dbService )
            : base( logger, config, webService, dbService )
        {
        }

        [HttpPost]
        [ApiExplorerSettings( GroupName = "client" )]
        [SwaggerExtend( "선수/코치 보관함 확장", typeof(ExtendCharacterInvenPacket) )]
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

            // 유저 정보 가져옴
            DataSet dataSet = gameDB.USP_GS_GM_ACCOUNT_GAME_ONLY_R(webSession.TokenInfo.Pcid);
            if (dataSet == null)
            {
                return _webService.End(ErrorCode.ERROR_DB, "USP_GS_GM_ACCOUNT_GAME_ONLY_R");
            }

            DataSetWrapper gameDataSetWrapper = new DataSetWrapper(dataSet);
            AccountGame accountGameInfo = gameDataSetWrapper.GetObject<AccountGame>(0);

            if (accountGameInfo == null)
            {
                return _webService.End(ErrorCode.ERROR_NO_ACCOUNT, "USP_GS_GM_ACCOUNT_GAME_ONLY_R");
            }

            ErrorCode extendErrorCode = Cache.CacheManager.PBTable.PlayerTable.InvenExtend(reqData.CharacterType, reqData.TryExtendLevel, accountGameInfo, out byte costType, out int costCount);
            if (extendErrorCode != ErrorCode.SUCCESS)
            {
                return _webService.End(extendErrorCode, "InvenExtend Fun");
            }

            ConsumeReward consumeProcess = new ConsumeReward( webSession.TokenInfo.Pcid, gameDB, Common.Define.CONSUME_REWARD_TYPE.CONSUME, false );
            consumeProcess.AddConsume(new GameRewardInfo(costType, 0, costCount) );
            ErrorCode consumeResult = consumeProcess.Run( ref accountGameInfo, true );
            if( consumeResult != ErrorCode.SUCCESS )
            {
                return _webService.End( consumeResult );
            }

            // 보상 처리
            if (gameDB.USP_GS_GM_REWARD_PROCESS(webSession.TokenInfo.Pcid, accountGameInfo, null) == false)
            {
                return _webService.End(ErrorCode.ERROR_DB, "USP_GS_GM_REWARD_PROCESS");
            }

            resData.PlayerInvenMaxCount = accountGameInfo.max_player;
            resData.CoachInvenMaxCount = accountGameInfo.max_coach;
            resData.CostType = costType;
            resData.CostCount = costCount;
            resData.AccountCurrency = accountGameInfo;

            return _webService.End();
        }
    }
}
