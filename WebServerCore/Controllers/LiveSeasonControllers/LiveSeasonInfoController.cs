using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using StackExchange.Redis.Extensions.Core;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Threading.Tasks;
using ApiWebServer.Cache;
using ApiWebServer.Common.Define;
using ApiWebServer.Core;
using ApiWebServer.Core.Controller;
using ApiWebServer.Core.Swagger;
using ApiWebServer.Database;
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
    public class LiveSeasonInfoController : SessionContoller<ReqLiveSeasonInfo, ResLiveSeasonInfo>
    {
        private readonly RankServer _rankServer;

        private static readonly byte NormalFlag = 0;
        private static readonly byte CreateFlag = 1;
        private static readonly byte UpdateFlag = 2;
        private static readonly byte ResetFlag = 3;

        public LiveSeasonInfoController(
           ILogger<LiveSeasonInfoController> logger,
           IConfiguration config,
           IWebService<ReqLiveSeasonInfo, ResLiveSeasonInfo> webService,
           IDBService dbService,
           ICacheClient redisClient)
           : base(logger, config, webService, dbService)
        {
            _rankServer = new RankServer(redisClient, logger);
        }

        [HttpPost]
        [ApiExplorerSettings(GroupName = "client")]
        [SwaggerExtend("라이브 시즌 전체 정보 조회", typeof(LiveSeasonInfoPacket))]
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
            var postDB = _dbService.CreatePostDB(_webService.RequestNo, webSession.DBNo);

            byte commendFlag = NormalFlag;
            bool isforcedGameEnd = false;
            byte rankModifyFlag = (byte)COMPETITION_RANK_FLAG.NONE;
            long ranking = 0;
            int lastRatingIdx = 0;
            long lastRanking = 0;

            List<GameRewardInfo> promotionReward = null;
            List<PostInsert> postRewardList = new List<PostInsert>();

            // 유효성 체크
            if (webSession.TeamIdx == 0)
            {
                return _webService.End(ErrorCode.ERROR_NOT_FOUND_TEAM_INFO);
            }

            // 등급전 시즌 정보 가져옴
            DataSet scheduleDataSet = accountDB.USP_AC_LIVESEASON_SCHEDULE_R();
            if (scheduleDataSet == null)
            {
                return _webService.End(ErrorCode.ERROR_DB, "USP_AC_LIVESEASON_SCHEDULE_R");
            }

            DataSetWrapper scheduleDataSetWrapper = new DataSetWrapper(scheduleDataSet);
            List<ScheduleInfo> scheduleInfo = scheduleDataSetWrapper.GetObjectList<ScheduleInfo>(0);

            // 스케줄 정보 셋팅
            List<SeasonSchedule> scheduleList = GetSeasonSchedule(scheduleInfo, out int competitionSeasonIdx, out int cycleSeasonIdx);

            // 유저 정보 가져옴
            DataSet dataSet = gameDB.USP_GS_GM_LIVESEASON_INFO_R(webSession.TokenInfo.Pcid);
            if (dataSet == null)
            {
                return _webService.End(ErrorCode.ERROR_DB, "USP_GS_GM_LIVESEASON_INFO_R");
            }

            DataSetWrapper dataSetWrapper = new DataSetWrapper(dataSet);
            CompetitionInfo competitionInfo = dataSetWrapper.GetObject<CompetitionInfo>(0);
            int totalGameNo = dataSetWrapper.GetValue(1, "total_game_no", 0);

            // 등급 경쟁전 시즌 진행 중
            if (competitionInfo != null)
            {
                if (competitionInfo.season_reward_idx > 0)
                {
                    // 이전 시즌정보 가져옴
                    DataSet recordDataSet = gameDB.USP_GS_GM_LIVESEASON_INFO_RECORD(webSession.TokenInfo.Pcid);
                    if (recordDataSet == null)
                    {
                        return _webService.End(ErrorCode.ERROR_DB, "USP_GS_GM_LIVESEASON_INFO_RECORD");
                    }

                    DataSetWrapper recordDataSetWrapper = new DataSetWrapper(recordDataSet);
                    lastRatingIdx = recordDataSetWrapper.GetValue<int>(0, "previous_rating_idx");
                    lastRanking = recordDataSetWrapper.GetValue<int>(0, "previous_ranking");
                }

                if (competitionInfo.promotion_reward_idx > 0)
                {
                    // 등급전 승급 보상
                    promotionReward = CacheManager.PBTable.LiveSeasonTable.GetPromotionRatingReward(competitionInfo.rating_idx);
                    competitionInfo.promotion_reward_idx = 0;
                    commendFlag = UpdateFlag;

                    postRewardList.Add(new PostInsert(webSession.PubId, promotionReward));

                }

                if (competitionInfo.battle_key != null)
                {
                    // 강제 종료로 인한 처리
                    isforcedGameEnd = true;
                    competitionInfo.battle_key = null;

                    // 강제 종료 패배 처리
                    int beforeRatingIdx = competitionInfo.rating_idx;
                    CacheManager.PBTable.LiveSeasonTable.LoseCompetition(competitionInfo, out int addExp, out bool isRankDown);
                    if (isRankDown == true)
                    {
                        string key = CacheManager.PBTable.LiveSeasonTable.GetMatchKey(competitionInfo.season_idx, beforeRatingIdx);
                        await _rankServer.RemoveScore(key, webSession.TokenInfo.Pcid);

                        competitionInfo.rank_modify_flag = (byte)COMPETITION_RANK_FLAG.RANK_DOWN;
                    }

                    commendFlag = UpdateFlag;
                }

                // 레전드일 경우 현재 랭킹 보여줌
                if (CacheManager.PBTable.LiveSeasonTable.IsLastRank(competitionInfo.rating_idx) == true)
                {
                    string rankKey = CacheManager.PBTable.LiveSeasonTable.GetRankKey(competitionInfo.season_idx);
                    ranking = await _rankServer.GetScoreRank(rankKey, webSession.TokenInfo.Pcid);
                }

                // 시즌 초기화 체크
                if (competitionInfo.season_idx != competitionSeasonIdx)
                {
                    // 게임 진행했을 시에만 초기화 진행
                    if (competitionInfo.game_no > 0)
                    {
                        // 시즌 정보 저장
                        lastRatingIdx = competitionInfo.rating_idx;
                        lastRanking = ranking;

                        // 등급전 시즌 초기화
                        CacheManager.PBTable.LiveSeasonTable.ResetCompetitionSeason(competitionInfo.rating_idx, out int resetRationgIdx);

                        competitionInfo.season_reward_idx = lastRatingIdx;
                        competitionInfo.rating_idx = resetRationgIdx;
                    }

                    competitionInfo.season_idx = competitionSeasonIdx;
                    competitionInfo.game_no = 0;
                    competitionInfo.point = 0;
                    competitionInfo.winning_streak = 0;

                    commendFlag = ResetFlag;
                }
            }
            else // 등급 경쟁전 처음 시즌 진행
            {
                competitionInfo = new CompetitionInfo()
                {
                    season_idx = competitionSeasonIdx,
                    rating_idx = 1
                };

                commendFlag = CreateFlag;
            }

            // 승급/강등 저장 정보 리턴
            if (competitionInfo.rank_modify_flag != (byte)COMPETITION_RANK_FLAG.NONE)
            {
                rankModifyFlag = competitionInfo.rank_modify_flag;
                competitionInfo.rank_modify_flag = 0;

                commendFlag = UpdateFlag;
            }

            if (commendFlag != NormalFlag)
            {
                // 정보 저장
                if (gameDB.USP_GS_GM_LIVESEASON_INFO(webSession.TokenInfo.Pcid, JsonConvert.SerializeObject(competitionInfo), commendFlag, lastRatingIdx, lastRanking) == false)
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
            }

            resData.GameNo = totalGameNo;
            resData.ScheduleInfo = scheduleList;
            resData.RatingIdx = competitionInfo.rating_idx;
            resData.Point = competitionInfo.point;
            resData.WinningStreak = competitionInfo.winning_streak;
            resData.ForcedGameEndFlag = (byte)(isforcedGameEnd ? 1 : 0);
            resData.RankModifyFlag = rankModifyFlag;
            resData.PromotionRewardInfo = promotionReward;
            resData.Ranking = ranking;
            resData.PreviousRatingIdx = lastRatingIdx;
            resData.PreviousRanking = lastRanking;

            return _webService.End();
        }

        private List<SeasonSchedule> GetSeasonSchedule(List<ScheduleInfo> scheduleInfo, out int competitionSeasonIdx, out int cycleSeasonIdx)
        {
            List<SeasonSchedule> scheduleList = new List<SeasonSchedule>();
            competitionSeasonIdx = 0;
            cycleSeasonIdx = 0;

            foreach (var schedule in scheduleInfo)
            {
                long nowTime = new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds();
                long startTime = new DateTimeOffset(TimeZoneInfo.ConvertTimeToUtc(DateTime.Parse(schedule.start_time))).ToUnixTimeSeconds();
                long endTime = new DateTimeOffset(TimeZoneInfo.ConvertTimeToUtc(DateTime.Parse(schedule.end_time))).ToUnixTimeSeconds();
                long readyTime = 0;
                long remainTime = 0;
                if (startTime <= nowTime && nowTime < endTime)
                {
                    remainTime = endTime - nowTime;
                }
                else if (nowTime < startTime)
                {
                    readyTime = startTime - nowTime;
                }

                scheduleList.Add(new SeasonSchedule()
                {
                    season_idx = schedule.season_idx,
                    schedule_idx = schedule.schedule_idx,
                    start_time = startTime,
                    end_time = endTime,
                    ready_time = readyTime,
                    remain_time = remainTime,
                    use_flag = schedule.use_flag
                });

                if (schedule.schedule_idx == (int)LIVESEASON_SCHEDULE_IDX.COMPETITION)
                {
                    competitionSeasonIdx = schedule.season_idx;
                }
                if (schedule.schedule_idx == (int)LIVESEASON_SCHEDULE_IDX.CYCLE)
                {
                    cycleSeasonIdx = schedule.season_idx;
                }
            }

            return scheduleList;
        }
    }
}
