using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using StackExchange.Redis;
using StackExchange.Redis.Extensions.Core;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using ApiWebServer.Cache;
using ApiWebServer.Common;
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
    public class CompetitionRankController : SessionContoller<ReqCompetitionRank, ResCompetitionRank>
    {
        private readonly RankServer _rankServer;

        public CompetitionRankController(
           ILogger<CompetitionRankController> logger,
           IConfiguration config,
           IWebService<ReqCompetitionRank, ResCompetitionRank> webService,
           IDBService dbService,
           ICacheClient redisClient)
           : base(logger, config, webService, dbService)
        {
            _rankServer = new RankServer(redisClient, logger);
        }

        [HttpPost]
        [ApiExplorerSettings(GroupName = "client")]
        [SwaggerExtend("경쟁전(등급전) 랭킹 조회", typeof(CompetitionRankPacket))]
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
            var accountDB = _dbService.CreateAccountDB(_webService.RequestNo);
            var gameDB = _dbService.CreateGameDB(_webService.RequestNo, webSession.DBNo);

            int getSeasonIdx = 0;

            // 등급전 시즌 정보 가져옴
            DataSet scheduleDataSet = accountDB.USP_AC_LIVESEASON_SCHEDULE_R();
            if (scheduleDataSet == null)
            {
                return _webService.End(ErrorCode.ERROR_DB, "USP_AC_LIVESEASON_SCHEDULE_R");
            }

            DataSetWrapper scheduleDataSetWrapper = new DataSetWrapper(scheduleDataSet);
            List<ScheduleInfo> scheduleInfo = scheduleDataSetWrapper.GetObjectList<ScheduleInfo>(0);
            foreach( var schedule in scheduleInfo)
            {
                if(schedule.schedule_idx == ( byte)LIVESEASON_SCHEDULE_IDX.COMPETITION)
                {
                    getSeasonIdx = schedule.season_idx;
                    break;
                }
            }

            // 유효성 체크
            if(reqData.SeasonIdx != getSeasonIdx && reqData.SeasonIdx != getSeasonIdx - 1)
            {
                return _webService.End(ErrorCode.ERROR_INVALID_SEASON_IDX);
            }
            if(reqData.EndRank - reqData.StartRank < 0)
            {
                return _webService.End(ErrorCode.ERROR_INVALID_REQUEST_RANK);
            }

            int nowSeasonIdx = CacheManager.CompetitonRanking.NowSeasonIdx;

            // 랭킹 처음 조회
            if(nowSeasonIdx == 0)
            {
                nowSeasonIdx = getSeasonIdx;
                int beforeSeasonIdx = nowSeasonIdx - 1;

                // 랭킹 데이터 가져옴
                List<RankingInfo> nowRankings = await GetRankingData(nowSeasonIdx);
                List<RankingInfo> beforeRankings = await GetRankingData(beforeSeasonIdx);

                CacheManager.CompetitonRanking.SetRankData(nowSeasonIdx, nowRankings);
                CacheManager.CompetitonRanking.SetRankData(beforeSeasonIdx, beforeRankings);

                int cacheTime = ServerUtils.GetConfigValue<int>(_config.GetSection("GlobalConfig"), "RedisRankCacheTime");
                CacheManager.CompetitonRanking.ExpiredCacheTime = ServerUtils.GetNowUtcTimeStemp(new TimeSpan(0, cacheTime, 0));
                CacheManager.CompetitonRanking.NowSeasonIdx = nowSeasonIdx;
            }
            else
            {
                // 이전 시즌 저장 및 현재 시즌 랭킹 저장
                if(nowSeasonIdx < getSeasonIdx)
                {
                    int removeSeasonIdx = nowSeasonIdx - 1;
                    CacheManager.CompetitonRanking.RemoveRankData(removeSeasonIdx);

                    nowSeasonIdx = getSeasonIdx;
                    List<RankingInfo> nowRankings = await GetRankingData(nowSeasonIdx);
                    CacheManager.CompetitonRanking.SetRankData(nowSeasonIdx, nowRankings);

                    int cacheTime = ServerUtils.GetConfigValue<int>(_config.GetSection("GlobalConfig"), "RedisRankCacheTime");
                    CacheManager.CompetitonRanking.ExpiredCacheTime = ServerUtils.GetNowUtcTimeStemp(new TimeSpan(0, cacheTime, 0));
                    CacheManager.CompetitonRanking.NowSeasonIdx = nowSeasonIdx;
                }
                else
                {
                    // 갱신 시간 체크
                    long nowTime = ServerUtils.GetNowUtcTimeStemp();
                    if (CacheManager.CompetitonRanking.ExpiredCacheTime < nowTime) // thread not safe 허용
                    {
                        // 랭킹 데이터 가져옴
                        List<RankingInfo> nowRankings = await GetRankingData(nowSeasonIdx);
                        CacheManager.CompetitonRanking.SetRankData(nowSeasonIdx, nowRankings);

                        int cacheTime = ServerUtils.GetConfigValue<int>(_config.GetSection("GlobalConfig"), "RedisRankCacheTime");
                        CacheManager.CompetitonRanking.ExpiredCacheTime = ServerUtils.GetNowUtcTimeStemp(new TimeSpan(0, cacheTime, 0));
                    }
                }
            }

            List<RankingInfo> rankingInfos = CacheManager.CompetitonRanking.GetRankData(reqData.SeasonIdx);

            int startIdx = reqData.StartRank - 1;
            int rangeCount = reqData.EndRank - reqData.StartRank + 1;

            if (rankingInfos.Count < rangeCount)
            {
                rangeCount = rankingInfos.Count;
            }

            resData.RankData = rankingInfos.GetRange(startIdx, rangeCount);

            return _webService.End();
        }

        private async Task<List<RankingInfo>> GetRankingData(int nowSeasonIdx)
        {
            List<RankingInfo> rankingList = new List<RankingInfo>();
            string rankKey = CacheManager.PBTable.LiveSeasonTable.GetRankKey(nowSeasonIdx);
            SortedSetEntry[] entryList = await _rankServer.GetScoreRankRange(rankKey);

            for (int i = 0; i < entryList.Length; ++i)
            {
                int nowRank = i + 1;
                rankingList.Add(new RankingInfo()
                {
                    pcId = (long)entryList[i].Element,
                    rank = nowRank,
                    score = entryList[i].Score,
                    info = await _rankServer.GetData<BattleInfo>(CacheManager.PBTable.LiveSeasonTable.GetBattleKey((long)entryList[i].Element))
                });
            }

            return rankingList;
        }
    }
}
