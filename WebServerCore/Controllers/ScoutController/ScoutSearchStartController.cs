using System.Collections.Generic;
using System.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using ApiWebServer.Core;
using ApiWebServer.Core.Controller;
using ApiWebServer.Core.Swagger;
using ApiWebServer.Database.Utils;
using WebSharedLib.Contents;
using WebSharedLib.Contents.Api;
using WebSharedLib.Core.NPLib;
using WebSharedLib.Entity;
using WebSharedLib.Error;
using ApiWebServer.Models;
using ApiWebServer.Logic;
using ApiWebServer.Common.Define;

namespace ApiWebServer.Controllers.ScoutControllers
{
    [Route("api/Scout/[controller]")]
    [ApiController]
    public class ScoutSearchStartController : SessionContoller<ReqScoutSearchStart, ResScoutSearchStart>
    {
        public ScoutSearchStartController(
            ILogger<ScoutSearchStartController> logger,
            IConfiguration config,
            IWebService<ReqScoutSearchStart, ResScoutSearchStart> webService, 
            IDBService dbService )
            : base( logger, config, webService, dbService )
        {
        }

        [HttpPost]
        [ApiExplorerSettings( GroupName = "client" )]
        [SwaggerExtend( "스카우트 탐색 시작", typeof(ScoutSearchStartPacket) )]
        public NPWebResponse Controller([FromBody] NPWebRequest requestBody )
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
            var gameDB = _dbService.CreateGameDB(_webService.RequestNo, webSession.DBNo);

            // 스카우트 정보 조회
            DataSet dataSet = gameDB.USP_GS_GM_SCOUT_SEARCH_START_R(webSession.TokenInfo.Pcid, reqData.SearchSlotIdx);
            if (dataSet == null)
            {
                return _webService.End( ErrorCode.ERROR_DB, "USP_GS_GM_SCOUT_SEARCH_START_R");
            }

            DataSetWrapper dataSetWrapper = new DataSetWrapper( dataSet );

            AccountScoutSlot scoutSlotInfo = dataSetWrapper.GetObject<AccountScoutSlot>(0);
            AccountGame accountGameInfo = dataSetWrapper.GetObject<AccountGame>(1);

            if (scoutSlotInfo == null)
            {
                return _webService.End(ErrorCode.ERROR_REQUEST_DATA, "not user slot row data");
            }

            //해당슬롯이 이미 진행중이면 에러
            if(scoutSlotInfo.character_type != (byte)SCOUT_USE_TYPE.NONE )
            {
                return _webService.End(ErrorCode.ERROR_ALREAY_SEARCH_START);
            }

            //슬롯 값 셋팅
            Cache.CacheManager.PBTable.ItemTable.SetScoutSearchStart(scoutSlotInfo, webSession.NationType, reqData.SearchCharacterType, out GameRewardInfo consumeCost);

            //재화차감
            ConsumeReward consumeProcess = new ConsumeReward(webSession.TokenInfo.Pcid, gameDB, CONSUME_REWARD_TYPE.CONSUME, false);
            consumeProcess.AddConsume(consumeCost);
            ErrorCode consumeResult = consumeProcess.Run(ref accountGameInfo, true);
            if (consumeResult != ErrorCode.SUCCESS)
            {
                return _webService.End(consumeResult);
            }

            if (gameDB.USP_GS_GM_SCOUT_SEARCH_START(webSession.TokenInfo.Pcid, scoutSlotInfo, accountGameInfo) == false)
            {
                return _webService.End(ErrorCode.ERROR_DB, "USP_GS_GM_SCOUT_SEARCH_START");
            }

            resData.SearchRemainSec = scoutSlotInfo.remain_sec;
            resData.ResultAccountCurrency = accountGameInfo;

            return _webService.End();
        }
    }
}
