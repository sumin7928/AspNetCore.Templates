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
    public class CompetitionDetailInfoController : SessionContoller<ReqCompetitionDetailInfo, ResCompetitionDetailInfo>
    {
        private readonly RankServer _rankServer;

        public CompetitionDetailInfoController(
           ILogger<CompetitionDetailInfoController> logger,
           IConfiguration config,
           IWebService<ReqCompetitionDetailInfo, ResCompetitionDetailInfo> webService,
           IDBService dbService,
           ICacheClient redisClient)
           : base(logger, config, webService, dbService)
        {
            _rankServer = new RankServer(redisClient, logger);
        }

        [HttpPost]
        [ApiExplorerSettings(GroupName = "client")]
        [SwaggerExtend("경쟁전(등급전) 상세 정보 조회", typeof(CompetitionDetailInfoPacket))]
        public async Task<NPWebResponse> Controller([FromBody] NPWebRequest requestBody)
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

            long targetPcid = webSession.TokenInfo.Pcid;
            string targetNickname = webSession.UserName;
            byte targetNationType = webSession.NationType;

            bool targetFlag = false;
            if (reqData.UserIdx > 0)
            {
                targetPcid = reqData.UserIdx;
                targetFlag = true;
            }

            long ranking = 0;
            List<int> rewardRatingList = null;

            // 정보 가져옴
            DataSet dataSet = gameDB.USP_GS_GM_LIVESEASON_COMPETITION_DETAIL_INFO_R(targetPcid, targetFlag);
            if (dataSet == null)
            {
                return _webService.End(ErrorCode.ERROR_DB, "USP_GS_GM_LIVESEASON_COMPETITION_DETAIL_INFO_R");
            }

            DataSetWrapper dataSetWrapper = new DataSetWrapper(dataSet);
            CompetitionInfo competitionInfo = dataSetWrapper.GetObject<CompetitionInfo>(0);
            CompetitionRecord competitionRecord = dataSetWrapper.GetObject<CompetitionRecord>(1);

            if ( targetFlag == false)
            {
                rewardRatingList = dataSetWrapper.GetValueList<int>(2, "rating_idx");
            }
            else
            {
                targetNickname = dataSetWrapper.GetValue<string>(2, "nick_name");
                targetNationType = dataSetWrapper.GetValue<byte>(2, "nation_type");
            }

            if (competitionInfo == null)
            {
                return _webService.End(ErrorCode.ERROR_NOT_FOUND_RATINGBATTLE_INFO);
            }

            // 레전드일 경우 현재 랭킹 보여줌
            if (Cache.CacheManager.PBTable.LiveSeasonTable.IsLastRank(competitionInfo.rating_idx) == true)
            {
                string rankKey = Cache.CacheManager.PBTable.LiveSeasonTable.GetRankKey(competitionInfo.season_idx);
                ranking = await _rankServer.GetScoreRank(rankKey, targetPcid);
            }

            resData.Rank = ranking;
            resData.RationgIdx = competitionInfo.rating_idx;
            resData.NationType = targetNationType;
            resData.Point = competitionInfo.point;
            resData.PreviousRationgIdx = competitionRecord.previous_rating_idx;
            resData.PreviousRanking = competitionRecord.previous_ranking;
            resData.TopRationgIdx = competitionRecord.top_rating_idx;
            resData.TopRanking = competitionRecord.top_ranking;
            resData.PreferredPlayer = competitionInfo.preferred_player;
            resData.PreferredCoach = competitionInfo.preferred_coach;
            resData.RewardRatingIdxList = rewardRatingList;

            return _webService.End();
        }
    }
}
