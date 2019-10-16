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
    public class CompetitionEndController : SessionContoller<ReqCompetitionEnd, ResCompetitionEnd>
    {
        private readonly RankServer _rankServer;

        public CompetitionEndController(
           ILogger<CompetitionEndController> logger,
           IConfiguration config,
           IWebService<ReqCompetitionEnd, ResCompetitionEnd> webService,
           IDBService dbService,
           ICacheClient redisClient)
           : base(logger, config, webService, dbService)
        {
            _rankServer = new RankServer(redisClient, logger);
        }

        [HttpPost]
        [ApiExplorerSettings(GroupName = "client")]
        [SwaggerExtend("경쟁전(등급전) 게임 종료", typeof(CompetitionEndPacket))]
        public async Task<NPWebResponse> Controller([FromBody] NPWebRequest requestBody)
        {
            WrapWebService(requestBody);
            if (_webService.ErrorCode != ErrorCode.SUCCESS)
            {
                return _webService.End(_webService.ErrorCode);
            }

            // 패킷 흐름 체크
            CheckUrlFlow(CompetitionStartPacket.apiURL);
            if (_webService.ErrorCode != ErrorCode.SUCCESS)
            {
                return _webService.End(_webService.ErrorCode);
            }

            // Business
            var webSession = _webService.WebSession;
            var reqData = _webService.WebPacket.ReqData;
            var resData = _webService.WebPacket.ResData;
            var gameDB = _dbService.CreateGameDB(_webService.RequestNo, webSession.DBNo);

            List<GameRewardInfo> gameReward = null;
            int addExp = 0;

            // 정보 가져옴
            DataSet dataSet = gameDB.USP_GS_GM_LIVESEASON_COMPETITION_END_R(webSession.TokenInfo.Pcid);
            if (dataSet == null)
            {
                return _webService.End(ErrorCode.ERROR_DB, "USP_GS_GM_LIVESEASON_RATINGBATTLE_START_R");
            }

            DataSetWrapper dataSetWrapper = new DataSetWrapper(dataSet);
            CompetitionInfo competitionInfo = dataSetWrapper.GetObject<CompetitionInfo>(0);
            AccountGame accountGameInfo = dataSetWrapper.GetObject<AccountGame>(1);
            int win = dataSetWrapper.GetValue<int>(2, "win");
            int draw = dataSetWrapper.GetValue<int>(2, "draw");
            int lose = dataSetWrapper.GetValue<int>(2, "lose");
            List<BattlePlayer> playerList = dataSetWrapper.GetObjectList<BattlePlayer>(3);
            List<BattleCoach> coachList = dataSetWrapper.GetObjectList<BattleCoach>(4);
            int totalGameNo = dataSetWrapper.GetValue<int>(5, "total_game_no");

            if (competitionInfo == null)
            {
                return _webService.End(ErrorCode.ERROR_NOT_FOUND_RATINGBATTLE_INFO);
            }

            competitionInfo.preferred_player = reqData.PreferredPlayer;
            competitionInfo.preferred_coach = reqData.PreferredCoach;
            competitionInfo.game_no += 1;

            // 대전 키 검증
            if (webSession.BattleKey == null || webSession.BattleKey != competitionInfo.battle_key)
            {
                return _webService.End(ErrorCode.ERROR_INVALID_BATTLE_KEY);
            }

            // 대전 키로 유효한 시간 체크
            int battleValidTime = ServerUtils.GetConfigValue<int>(_config.GetSection("Config"), "BattleValidTime");
            if (KeyGenerator.Instance.ValidateKey(GAME_KEY_TYPE.COMPETITION_GAME, webSession.BattleKey, battleValidTime) == false)
            {
                return _webService.End(ErrorCode.ERROR_INVALID_BATTLE_KEY);
            }

            // 대전 결과 처리
            if (reqData.GameResult == (byte)GAME_RESULT.WIN)
            {
                int beforeRatingIdx = competitionInfo.rating_idx;
                gameReward = CacheManager.PBTable.LiveSeasonTable.WinCompetition(competitionInfo, out addExp, out bool isRankUp);
                if (isRankUp == true)
                {
                    string key = CacheManager.PBTable.LiveSeasonTable.GetMatchKey(competitionInfo.season_idx, beforeRatingIdx);
                    await _rankServer.RemoveScore(key, webSession.TokenInfo.Pcid);

                    competitionInfo.rank_modify_flag = (byte)COMPETITION_RANK_FLAG.RANK_UP;
                }
            }
            else if (reqData.GameResult == (byte)GAME_RESULT.DRAW)
            {
                gameReward = CacheManager.PBTable.LiveSeasonTable.DropCompetition(competitionInfo, out addExp);
            }
            else if (reqData.GameResult == (byte)GAME_RESULT.LOSE)
            {
                int beforeRatingIdx = competitionInfo.rating_idx;
                gameReward = CacheManager.PBTable.LiveSeasonTable.LoseCompetition(competitionInfo, out addExp, out bool isRankDown);
                if (isRankDown == true)
                {
                    string key = CacheManager.PBTable.LiveSeasonTable.GetMatchKey(competitionInfo.season_idx, beforeRatingIdx);
                    await _rankServer.RemoveScore(key, webSession.TokenInfo.Pcid);

                    // 이전 레전드 등급일 시 삭제
                    if (CacheManager.PBTable.LiveSeasonTable.IsLastRank(beforeRatingIdx) == true)
                    {
                        string rankKey = CacheManager.PBTable.LiveSeasonTable.GetRankKey(competitionInfo.season_idx);
                        await _rankServer.RemoveScore(rankKey, webSession.TokenInfo.Pcid);
                    }

                    competitionInfo.rank_modify_flag = (byte)COMPETITION_RANK_FLAG.RANK_DOWN;
                }

            }
            else
            {
                return _webService.End(ErrorCode.ERROR_INVALID_GAME_RESULT);
            }

            // 레전드 등급일 시 포인트 저장 ( 변경된 포인트만 )
            if (CacheManager.PBTable.LiveSeasonTable.IsLastRank(competitionInfo.rating_idx) == true
                && reqData.GameResult != (byte)GAME_RESULT.DRAW)
            {
                string rankKey = CacheManager.PBTable.LiveSeasonTable.GetRankKey(competitionInfo.season_idx);
                bool isExistedKey = await _rankServer.IsExistKey(rankKey);
                await _rankServer.SetScore(rankKey, webSession.TokenInfo.Pcid, competitionInfo.point);

                if (isExistedKey == false)
                {
                    int rankExpiredDate = ServerUtils.GetConfigValue<int>(_config.GetSection("GlobalConfig"), "RedisRankExpiredTime");
                    await _rankServer.SetExpiredTime(rankKey, new TimeSpan(rankExpiredDate, 0, 0, 0));
                }
            }

            // 승급 보상이 있을경우
            if (competitionInfo.promotion_reward_idx > 0)
            {
                // 이미 보상 받은 정보 가져옴
                DataSet rewardDataSet = gameDB.USP_GS_GM_LIVESEASON_COMPETITION_END_REWARD(webSession.TokenInfo.Pcid, competitionInfo.promotion_reward_idx);
                if (rewardDataSet == null)
                {
                    return _webService.End(ErrorCode.ERROR_DB, "USP_GS_GM_LIVESEASON_RATINGBATTLE_START_R");
                }

                DataSetWrapper rewardDataSetWrapper = new DataSetWrapper(rewardDataSet);
                bool rewardFlag = rewardDataSetWrapper.GetValue<bool>(0, "reward_flag");
                if (rewardFlag == true)
                {
                    competitionInfo.promotion_reward_idx = 0;
                }
            }

            // 보상 정보 처리
            ConsumeReward reward = new ConsumeReward(webSession.TokenInfo.Pcid, gameDB, CONSUME_REWARD_TYPE.REWARD, true);
            reward.AddReward(gameReward);
            ErrorCode rewardResult = reward.Run(ref accountGameInfo, false);
            if (rewardResult != ErrorCode.SUCCESS)
            {
                return _webService.End(rewardResult);
            }

            // 경험치 처리
            LevelUp levelUp = new LevelUp(accountGameInfo);
            levelUp.AddExp(addExp, out bool isLevelUp);

            // 레벨업 시 처리
            if (isLevelUp)
            {

            }

            // 선수 체력 처리
            List<UpdatedHealthInfo> updatedHealthList = new List<UpdatedHealthInfo>();
            if (reqData.PlayingPitcherList != null)
            {
                foreach (var pitcher in reqData.PlayingPitcherList)
                {
                    updatedHealthList.Add(new UpdatedHealthInfo()
                    {
                        account_player_idx = pitcher.account_player_idx,
                        player_health = pitcher.player_health,
                        health_game_no = totalGameNo
                    });
                }
            }

            // 기록 byte 변환 처리는 추후 구현
            byte[] batterRecords = null;
            byte[] pitcherRecords = null;

            // 내 배틀 정보 저장
            string battleInfoKey = CacheManager.PBTable.LiveSeasonTable.GetBattleKey(webSession.TokenInfo.Pcid);
            BattleInfo battleInfo = new BattleInfo()
            {
                team_Idx = webSession.TeamIdx,
                nation_type = webSession.NationType,
                level = accountGameInfo.user_lv,
                nick_name = webSession.UserName,
                battle_time = ServerUtils.GetNowUtcTimeStemp(),
                win = win,
                draw = draw,
                lose = lose,
                player_list = playerList,
                coach_list = coachList
            };

            // 배틀 정보 저장
            int battleInfoExpiredDate = ServerUtils.GetConfigValue(_config.GetSection("GlobalConfig"), "RedisBattleInfoExpiredTime", LiveSeasonDefine.RedisBattleInfoExpiredTime);
            await _rankServer.SetData(battleInfoKey, battleInfo, new TimeSpan(battleInfoExpiredDate, 0, 0, 0));

            string updatePlayer = JsonConvert.SerializeObject(updatedHealthList);
            string accountJson = JsonConvert.SerializeObject(accountGameInfo);
            string updateItemList = reward.GetUpdateItemList() != null ? JsonConvert.SerializeObject(reward.GetUpdateItemList()) : string.Empty;

            if (gameDB.USP_GS_GM_LIVESEASON_COMPETITION_END(webSession.TokenInfo.Pcid, JsonConvert.SerializeObject(competitionInfo), competitionInfo.promotion_reward_idx,
                reqData.GameResult, accountJson, updateItemList, batterRecords, pitcherRecords, updatePlayer) == false)
            {
                return _webService.End(ErrorCode.ERROR_DB, "USP_GS_GM_LIVESEASON_RATINGBATTLE_START_R");
            }

            webSession.BattleKey = null;

            resData.AddManagerExp = addExp;
            resData.AddManagerBonusExp = 0;

            resData.ManagerExp = accountGameInfo.user_exp;
            resData.ManagerLv = accountGameInfo.user_lv;

            resData.RewardInfo = gameReward;
            resData.ResultAccountCurrency = accountGameInfo;
            resData.UpdateItemInfo = reward.GetUpdateItemList();

            return _webService.End();
        }
    }
}
