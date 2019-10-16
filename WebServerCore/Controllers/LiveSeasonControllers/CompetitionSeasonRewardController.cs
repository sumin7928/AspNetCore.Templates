using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using StackExchange.Redis.Extensions.Core;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
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

namespace ApiWebServer.Controllers.LiveSeasonControllers
{
    [Route("api/LiveSeason/[controller]")]
    [ApiController]
    public class CompetitionSeasonRewardController : SessionContoller<ReqCompetitionSeasonReward, ResCompetitionSeasonReward>
    {
        private readonly RankServer _rankServer;

        public CompetitionSeasonRewardController(
           ILogger<CompetitionSeasonRewardController> logger,
           IConfiguration config,
           IWebService<ReqCompetitionSeasonReward, ResCompetitionSeasonReward> webService,
           IDBService dbService,
           ICacheClient redisClient)
           : base(logger, config, webService, dbService)
        {
            _rankServer = new RankServer(redisClient, logger);
        }

        [HttpPost]
        [ApiExplorerSettings(GroupName = "client")]
        [SwaggerExtend("경쟁전(등급전) 시즌 보상 요청", typeof(CompetitionSeasonRewardPacket))]
        public NPWebResponse Controller([FromBody] NPWebRequest requestBody)
        {
            WrapWebService(requestBody);
            if (_webService.ErrorCode != ErrorCode.SUCCESS)
            {
                return _webService.End(_webService.ErrorCode);
            }

            // Business
            var webSession = _webService.WebSession;
            var reqData = _webService.WebPacket.ReqData;
            var resData = _webService.WebPacket.ResData;
            var gameDB = _dbService.CreateGameDB(_webService.RequestNo, webSession.DBNo);
            var postDB = _dbService.CreatePostDB(_webService.RequestNo, webSession.DBNo);

            List<GameRewardInfo> seasonRewardList = null;
            List<PostInsert> postRewardList = new List<PostInsert>();


            // 정보 가져옴
            DataSet dataSet = gameDB.USP_GS_GM_LIVESEASON_COMPETITION_SEASON_REWARD_R(webSession.TokenInfo.Pcid);
            if (dataSet == null)
            {
                return _webService.End(ErrorCode.ERROR_DB, "USP_GS_GM_LIVESEASON_COMPETITION_SEASON_REWARD_R");
            }

            DataSetWrapper dataSetWrapper = new DataSetWrapper(dataSet);
            int seasonRewardIdx = dataSetWrapper.GetValue<int>(0, "season_reward_idx");

            // 시즌 보상 진행
            if (seasonRewardIdx > 0)
            {
                seasonRewardList = Cache.CacheManager.PBTable.LiveSeasonTable.GetCompetitionSeasonReward(seasonRewardIdx);
                postRewardList.Add(new PostInsert(webSession.PubId, seasonRewardList));
            }

            // 정보 저장
            if (gameDB.USP_GS_GM_LIVESEASON_COMPETITION_SEASON_REWARD(webSession.TokenInfo.Pcid) == false)
            {
                return _webService.End(ErrorCode.ERROR_DB, "USP_GS_GM_LIVESEASON_INFO");
            }

            // 보상 정보 처리 ( 트랜젝션 처리는 나중에 체크.. )
            if (postRewardList.Count > 0)
            {
                foreach (var postInsert in postRewardList)
                {
                    if (postDB.USP_GS_PO_POST_SEND(webSession.TokenInfo.Pcid, webSession.UserName, -1, "admin", postInsert, (byte)POST_ADD_TYPE.ONE_BY_ONE) == false)
                    {
                        return _webService.End(ErrorCode.ERROR_DB, "USP_GS_PO_POST_SEND");
                    }
                }
            }

            resData.SeasonRewardList = seasonRewardList;

            return _webService.End();
        }
    }
}
