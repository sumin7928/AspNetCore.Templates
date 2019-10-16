using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using ApiWebServer.Core;
using ApiWebServer.Core.Controller;
using ApiWebServer.Core.Swagger;
using ApiWebServer.Logic;
using ApiWebServer.Models;
using WebSharedLib.Contents;
using WebSharedLib.Contents.Api;
using WebSharedLib.Core.NPLib;
using WebSharedLib.Entity;
using WebSharedLib.Error;
using ApiWebServer.Database.Utils;

namespace ApiWebServer.Controllers.MissionAchievementControllers
{
    [Route("api/MissionAchievement/[controller]")]
    [ApiController]
    public class MissionRewardController : SessionContoller<ReqMissionReward, ResMissionReward>
    {
        public MissionRewardController(
            ILogger<MissionRewardController> logger,
            IConfiguration config, 
            IWebService<ReqMissionReward, ResMissionReward> webService, 
            IDBService dbService )
            : base( logger, config, webService, dbService )
        {
        }

        [HttpPost]
        [ApiExplorerSettings(GroupName = "client")]
        [SwaggerExtend("미션 보상", typeof(MissionRewardPacket) )]
        public NPWebResponse Controller([FromBody] NPWebRequest requestBody)
        {
            WrapWebService( requestBody );
            if (_webService.ErrorCode != ErrorCode.SUCCESS)
            {
                return _webService.End( _webService.ErrorCode );
            }

            // Business
            var webSession = _webService.WebSession;
            var reqData = _webService.WebPacket.ReqData;
            var resData = _webService.WebPacket.ResData;
            var gameDB = _dbService.CreateGameDB(_webService.RequestNo, webSession.DBNo);

            if(reqData.MissionRewardList == null || reqData.MissionRewardList.Count == 0)
            {
                return _webService.End(ErrorCode.ERROR_INVALID_PARAM);
            }

            string finishIdxs = Common.ServerUtils.MakeSplittedString(reqData.MissionRewardList);

            // 클라 요청에 따른 내 미션/업적 가져오기.
            DataSet gameDataSet = gameDB.USP_GS_GM_ACCOUNT_MISSION_REWARD_R(webSession.TokenInfo.Pcid, finishIdxs);
            if (gameDataSet == null)
            {
                return _webService.End(ErrorCode.ERROR_DB, "USP_GS_GM_ACCOUNT_MISSION_REWARD_R");
            }

            DataSetWrapper gameDataSetWrapper = new DataSetWrapper(gameDataSet);

            if (gameDataSetWrapper.GetRowCount(0) == 0)
            {
                return _webService.End(ErrorCode.ERROR_STATIC_DATA, "not db day week idx ");
            }

            int dayIdx = gameDataSetWrapper.GetValue<int>(0, "day_idx");
            int weekIdx = gameDataSetWrapper.GetValue<int>(0, "week_idx");

            AccountGame accountGameInfo = gameDataSetWrapper.GetObject<AccountGame>(1);
            List<RepeatMission> finishMissionList = gameDataSetWrapper.GetObjectList<RepeatMission>(2);

            if (dayIdx != accountGameInfo.day_idx || weekIdx != accountGameInfo.week_idx)
            {
                return _webService.End(ErrorCode.ERROR_NOT_MISSION_LIST_RELOAD);
            }

            if (reqData.MissionRewardList.Count != finishMissionList.Count)
            {
                return _webService.End(ErrorCode.ERROR_MISSION_NOT_EXISTS_DATA);
            }

            // 미션 달성 가능한지 체크하고 가능하면 보상 내역 가져온다.
            if (Cache.CacheManager.PBTable.MissionAchievementTable.MissionRewardCheck(finishMissionList, out List<GameRewardInfo> missionRewardInfo) == false)
            {
                return _webService.End(ErrorCode.ERROR_MISSION_INVALID_INFO);
            }

            //실제 보상지급.
            ConsumeReward rewardProc = new ConsumeReward(webSession.TokenInfo.Pcid, gameDB, Common.Define.CONSUME_REWARD_TYPE.REWARD, false);
            rewardProc.AddReward(missionRewardInfo);
            ErrorCode rewardResult = rewardProc.Run(ref accountGameInfo, false);
            if (rewardResult != ErrorCode.SUCCESS)
            {
                return _webService.End(rewardResult);
            }

            //보상 내역 미션 테이블에 업데이트
            if (gameDB.USP_GS_GM_ACCOUNT_MISSION_REWARD(webSession.TokenInfo.Pcid, finishIdxs, accountGameInfo, rewardProc.GetUpdateItemList()) == false)
            {
                return _webService.End(ErrorCode.ERROR_DB, "USP_GS_GM_ACCOUNT_MISSION_REWARD");
            }

            //세션에서 완료된 미션리스트가 있다면 지워주자 (일단보류 카운트다되면 카운트올리는데서 지울꺼니까)
            //webSession.MissionList.RemoveAll(x => reqData.MissionRewardList.Contains(x.idx) == true);

            resData.RewardInfo = missionRewardInfo;
            resData.ResultAccountCurrency = accountGameInfo;
            resData.UpdateItemInfo = rewardProc.GetUpdateItemList();
           

            return _webService.End();
        }
    }
}
