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
    public class AchievementRewardController : SessionContoller<ReqAchievementReward, ResAchievementReward>
    {
        public AchievementRewardController(
            ILogger<AchievementRewardController> logger,
            IConfiguration config, 
            IWebService<ReqAchievementReward, ResAchievementReward> webService, 
            IDBService dbService )
            : base( logger, config, webService, dbService )
        {
        }

        [HttpPost]
        [ApiExplorerSettings(GroupName = "client")]
        [SwaggerExtend("업적 보상", typeof(AchievementRewardPacket) )]
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

            if(reqData.AchievementList == null || reqData.AchievementList.Count == 0)
            {
                return _webService.End(ErrorCode.ERROR_INVALID_PARAM);
            }

            string finishIdxs = Common.ServerUtils.MakeSplittedString(reqData.AchievementList);

            // 클라 요청에 따른 내 미션/업적 가져오기.
            DataSet gameDataSet = gameDB.USP_GS_GM_ACCOUNT_ACHIEVEMENT_REWARD_R(webSession.TokenInfo.Pcid, finishIdxs);
            if (gameDataSet == null)
            {
                return _webService.End(ErrorCode.ERROR_DB, "USP_GS_GM_ACCOUNT_ACHIEVEMENT_REWARD_R");
            }

            DataSetWrapper gameDataSetWrapper = new DataSetWrapper(gameDataSet);

            AccountGame accountGameInfo = gameDataSetWrapper.GetObject<AccountGame>(0);
            List<Achievement> finishAchievementList = gameDataSetWrapper.GetObjectList<Achievement>(1);

            if (reqData.AchievementList.Count != finishAchievementList.Count)
            {
                return _webService.End(ErrorCode.ERROR_ACHIEVEMENT_NOT_EXISTS_DATA);
            }

            // 미션 달성 가능한지 체크하고 가능하면 보상 내역 가져온다.
            if (Cache.CacheManager.PBTable.MissionAchievementTable.AchievementRewardCheck(finishAchievementList, out List<GameRewardInfo> achievementRewardInfo, out List<AchievementNextIdx> nextAchievementList) == false)
            {
                return _webService.End(ErrorCode.ERROR_ACHIEVEMENT_INVALID_INFO);
            }

            //실제 보상지급.
            ConsumeReward rewardProc = new ConsumeReward(webSession.TokenInfo.Pcid, gameDB, Common.Define.CONSUME_REWARD_TYPE.REWARD, false);
            rewardProc.AddReward(achievementRewardInfo);
            ErrorCode rewardResult = rewardProc.Run(ref accountGameInfo, false);
            if (rewardResult != ErrorCode.SUCCESS)
            {
                return _webService.End(rewardResult);
            }

            //보상 내역 미션 테이블에 업데이트
            if (gameDB.USP_GS_GM_ACCOUNT_ACHIEVEMENT_REWARD(webSession.TokenInfo.Pcid, nextAchievementList, accountGameInfo, rewardProc.GetUpdateItemList()) == false)
            {
                return _webService.End(ErrorCode.ERROR_DB, "USP_GS_GM_ACCOUNT_ACHIEVEMENT_REWARD");
            }

            //세션처리 (카운트때 처리할수도 있으므로 보류)
            /*foreach (AchievementNextIdx info in nextAchievementList)
            {
                Achievement obj = webSession.AchievementList.Find(x => x.idx == info.idx);
                if (obj != null)
                {
                    
                    if (info.nextIdx == 0)
                    {
                        //더이상 카운팅 할필요가 없으므로 삭제
                        webSession.AchievementList.Remove(obj);
                    }
                    else
                    {
                        obj.idx = info.nextIdx;
                        obj.count = info.count;
                    }
                }
            }*/

            resData.RewardInfo = achievementRewardInfo;
            resData.ResultAccountCurrency = accountGameInfo;
            resData.UpdateItemInfo = rewardProc.GetUpdateItemList();
           
            return _webService.End();
        }
    }
}
