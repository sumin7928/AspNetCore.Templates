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
    public class ScoutBinderResetController : SessionContoller<ReqScoutBinderReset, ResScoutBinderReset>
    {
        public ScoutBinderResetController(
            ILogger<ScoutBinderResetController> logger,
            IConfiguration config,
            IWebService<ReqScoutBinderReset, ResScoutBinderReset> webService, 
            IDBService dbService )
            : base( logger, config, webService, dbService )
        {
        }

        [HttpPost]
        [ApiExplorerSettings( GroupName = "client" )]
        [SwaggerExtend( "스카우트 바인더 리셋", typeof(ScoutBinderResetPacket) )]
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

            //최대 리셋횟수 체크
            if(reqData.BinderResetCount > Cache.CacheManager.PBTable.ConstantTable.Const.binder_reset_cost_maxnum)
            {
                return _webService.End(ErrorCode.ERROR_ALREADY_MAX_RESET);
            }

            // 스카우트 정보 조회
            DataSet dataSet = gameDB.USP_GS_GM_SCOUT_BINDER_RESET_R(webSession.TokenInfo.Pcid);
            if (dataSet == null)
            {
                return _webService.End( ErrorCode.ERROR_DB, "USP_GS_GM_SCOUT_BINDER_RESET_R");
            }

            DataSetWrapper dataSetWrapper = new DataSetWrapper( dataSet );

            if(dataSetWrapper.GetRowCount(0) == 0)
            {
                return _webService.End(ErrorCode.ERROR_STATIC_DATA, "scout_binder_static_data");
            }

            int dateNo = dataSetWrapper.GetValue<int>(0, "no");
            int remainSec = dataSetWrapper.GetValue<int>(0, "remain_sec");

            if(remainSec < 0)
            {
                return _webService.End(ErrorCode.ERROR_DB_DATA, "remainSec error");
            }

            AccountScoutBinder scoutBinderInfo = dataSetWrapper.GetObject<AccountScoutBinder>(1);
            AccountGame accountGameInfo = dataSetWrapper.GetObject<AccountGame>(2);

            if (scoutBinderInfo == null)
            {
                return _webService.End(ErrorCode.ERROR_REQUEST_DATA, "not user binder row data");
            }

            //유효성체크
            if (scoutBinderInfo.date_no != dateNo || scoutBinderInfo.reset_count + 1 != reqData.BinderResetCount)
            {
                return _webService.End(ErrorCode.ERROR_MATCHING_BINDER_INFO);
            }


            ErrorCode errorCode = Cache.CacheManager.PBTable.ItemTable.SetScoutBinderInfo(scoutBinderInfo, dateNo, webSession.NationType, true);
            if(errorCode != ErrorCode.SUCCESS)
            {
                return _webService.End(errorCode, "SetScoutBinderInfo Error");
            }

            //재화 또는 아이템 차감
            ConsumeReward consumeProcess = new ConsumeReward(webSession.TokenInfo.Pcid, gameDB, CONSUME_REWARD_TYPE.CONSUME, false);
            if(reqData.IsItemUse == true)
                consumeProcess.AddConsume(new GameRewardInfo((byte)REWARD_TYPE.NORMAL_ITEM, Cache.CacheManager.PBTable.ItemTable.itemIdxScoutBinderReset, 1));
            else
                consumeProcess.AddConsume(new GameRewardInfo((byte)Cache.CacheManager.PBTable.ConstantTable.Const.binder_reset_cost_type, 0, Cache.CacheManager.PBTable.ItemTable.GetBinderResetCost(scoutBinderInfo.reset_count)));

            ErrorCode consumeResult = consumeProcess.Run(ref accountGameInfo, false);
            if (consumeResult != ErrorCode.SUCCESS)
            {
                return _webService.End(consumeResult);
            }

            if (gameDB.USP_GS_GM_SCOUT_BINDER_RESET(webSession.TokenInfo.Pcid, scoutBinderInfo, accountGameInfo, consumeProcess.GetUpdateItemList()) == false)
            {
                return _webService.End(ErrorCode.ERROR_DB, "USP_GS_GM_SCOUT_BINDER_RESET");
            }

            resData.BinderResetRemainSec = remainSec;
            resData.UserBinderInfo = scoutBinderInfo;
            resData.ResultAccountCurrency = accountGameInfo;
            resData.UpdateItemInfo = consumeProcess.GetUpdateItemList();

            return _webService.End();
        }
    }
}
