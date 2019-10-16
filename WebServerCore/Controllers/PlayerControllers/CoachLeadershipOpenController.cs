using System.Collections.Generic;
using System.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using ApiWebServer.Cache;
using ApiWebServer.Common.Define;
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

namespace ApiWebServer.Controllers.PlayerControllers
{
    [Route("api/Player/[controller]")]
    [ApiController]
    public class CoachLeadershipOpenController : SessionContoller<ReqCoachLeadershipOpen, ResCoachLeadershipOpen>
    {
        public CoachLeadershipOpenController(
            ILogger<CoachLeadershipOpenController> logger,
            IConfiguration config, 
            IWebService<ReqCoachLeadershipOpen, ResCoachLeadershipOpen> webService, 
            IDBService dbService )
            : base( logger, config, webService, dbService )
        {
        }

        [HttpPost]
        [ApiExplorerSettings( GroupName = "client" )]
        [SwaggerExtend( "코치 능력(리더십) 오픈 / 재오픈", typeof(CoachLeadershipOpenPacket) )]
        public NPWebResponse Controller( [FromBody] NPWebRequest requestBody )
        {
            WrapWebService( requestBody );
            if ( _webService.ErrorCode != ErrorCode.SUCCESS)
            {
                return _webService.End( _webService.ErrorCode );
            }

            // Business
            var webSession = _webService.WebSession;
            var reqData = _webService.WebPacket.ReqData;
            var resData = _webService.WebPacket.ResData;
            var gameDB = _dbService.CreateGameDB(_webService.RequestNo, webSession.DBNo);

            DataSet dataSet = gameDB.USP_GS_GM_COACH_LEADERSHIP_OPEN_R(webSession.TokenInfo.Pcid, reqData.AccountCoachIdx);
            if (dataSet == null)
            {
                return _webService.End(ErrorCode.ERROR_DB, "USP_GS_GM_COACH_LEADERSHIP_OPEN_R");
            }

            DataSetWrapper dataSetWrapper = new DataSetWrapper(dataSet);
            AccountGame accountGameInfo = dataSetWrapper.GetObject<AccountGame>(0);
            AccountCoach accountCoach = dataSetWrapper.GetObject<AccountCoach>(1);
            AccountCoachLeadershipInfo accountCoachLeadershipInfo = dataSetWrapper.GetObject<AccountCoachLeadershipInfo>(2);
            byte already_flag = dataSetWrapper.GetValue<byte>(3, "use_flag");

            List<GameRewardInfo> consumeList = new List<GameRewardInfo>();

            // 제약 조건 확인
            if (accountGameInfo == null)
            {
                return _webService.End(ErrorCode.ERROR_NO_ACCOUNT);
            }

            if (accountCoach == null)
            {
                return _webService.End(ErrorCode.ERROR_NOT_COACH);
            }

            if (accountCoachLeadershipInfo == null)
            {
                return _webService.End(ErrorCode.ERROR_INVALID_COACH_LEADERSHIP);
            }
            /* 임시처리
            //이미 진행하고 있는 훈련이 있다면 에러
            if (already_flag == 1)
            {
                return _webService.End(ErrorCode.ERROR_REQUEST_DATA);
            }*/

            ErrorCode leadershipOpenResult = CacheManager.PBTable.PlayerTable.CoachLeadershipOpenCheck(reqData, accountCoach, accountCoachLeadershipInfo, out int leadershipIdx);

            if (leadershipOpenResult != ErrorCode.SUCCESS)
            {
                return _webService.End(leadershipOpenResult);
            }

            if (reqData.ReOpenFlag == true)
            {
                consumeList.Add(new GameRewardInfo((byte)CacheManager.PBTable.ConstantTable.Const.leadership_reset_cost_type1, 0, CacheManager.PBTable.ConstantTable.Const.leadership_reset_cost_count1));
                consumeList.Add(new GameRewardInfo((byte)CacheManager.PBTable.ConstantTable.Const.leadership_reset_cost_type2, 0, CacheManager.PBTable.ConstantTable.Const.leadership_reset_cost_count2));
            }
            else
            {
                consumeList.Add(new GameRewardInfo((byte)CacheManager.PBTable.ConstantTable.Const.leadership_open_cost_type1, 0, CacheManager.PBTable.ConstantTable.Const.leadership_open_cost_count1));
                consumeList.Add(new GameRewardInfo((byte)CacheManager.PBTable.ConstantTable.Const.leadership_open_cost_type2, 0, CacheManager.PBTable.ConstantTable.Const.leadership_open_cost_count2));
            }
            ConsumeReward consumeProcess = new ConsumeReward(webSession.TokenInfo.Pcid, gameDB, CONSUME_REWARD_TYPE.CONSUME, false);
            consumeProcess.AddConsume(consumeList);
            ErrorCode consumeResult = consumeProcess.Run(ref accountGameInfo, true);

            if (consumeResult != ErrorCode.SUCCESS)
            {
                return _webService.End(consumeResult);
            }

            if (gameDB.USP_GS_GM_COACH_LEADERSHIP_OPEN(webSession.TokenInfo.Pcid, reqData.AccountCoachIdx, reqData.ReOpenFlag, reqData.SlotIdx, leadershipIdx, accountGameInfo) == false)
            {
                return _webService.End(ErrorCode.ERROR_DB, "USP_GS_GM_COACH_LEADERSHIP_OPEN");
            }

            resData.ResultLeadershipIdx = leadershipIdx;
            resData.ResultAccountCurrency = accountGameInfo;

            return _webService.End();
        }
    }
}
