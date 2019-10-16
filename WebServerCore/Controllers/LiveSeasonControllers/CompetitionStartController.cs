using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
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
    public class CompetitionStartController : SessionContoller<ReqCompetitionStart, ResCompetitionStart>
    {
        private readonly RankServer _rankServer;

        public CompetitionStartController(
           ILogger<CompetitionStartController> logger,
           IConfiguration config,
           IWebService<ReqCompetitionStart, ResCompetitionStart> webService,
           IDBService dbService,
           ICacheClient redisClient)
           : base(logger, config, webService, dbService)
        {
            _rankServer = new RankServer(redisClient, logger);
        }

        [HttpPost]
        [ApiExplorerSettings(GroupName = "client")]
        [SwaggerExtend("경쟁전(등급전) 게임 시작", typeof(CompetitionStartPacket))]
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

            // 정보 가져옴
            DataSet dataSet = gameDB.USP_GS_GM_LIVESEASON_COMPETITION_START_R(webSession.TokenInfo.Pcid);
            if (dataSet == null)
            {
                return _webService.End(ErrorCode.ERROR_DB, "USP_GS_GM_LIVESEASON_RATINGBATTLE_START_R");
            }

            DataSetWrapper dataSetWrapper = new DataSetWrapper(dataSet);
            CompetitionInfo competitionInfo = dataSetWrapper.GetObject<CompetitionInfo>(0);
            CompetitionRecord competitionRecord = dataSetWrapper.GetObject<CompetitionRecord>(1);

            Queue<long> matchHistory = null;

            // 유효성 체크 ( 시작시에는 시즌인덱스 검증을 안함( 시즌이 넘어갈 타이밍에 에러를 줄 수 없음 ) )
            if (competitionInfo == null)
            {
                return _webService.End(ErrorCode.ERROR_NOT_FOUND_RATINGBATTLE_INFO);
            }
            if (competitionInfo.battle_key != null)
            {
                return _webService.End(ErrorCode.ERROR_ALREADY_START_COMPETITION_BATTLE);
            }
            if (competitionInfo.season_reward_idx > 0)
            {
                return _webService.End(ErrorCode.ERROR_NOT_COMPLETE_SEASON_REWARD);
            }

            // 매칭된 유저 정보 가져옴
            if (competitionRecord.match_history == null)
            {
                matchHistory = new Queue<long>();
            }
            else
            {
                matchHistory = JsonConvert.DeserializeObject<Queue<long>>(competitionRecord.match_history);
            }

            // 매칭 진행
            BattleInfo matchBattleInfo = null;
            string matchKey = CacheManager.PBTable.LiveSeasonTable.GetMatchKey(competitionInfo.season_idx, competitionInfo.rating_idx);

            // 매칭 타겟 확인
            byte matchingTarget = CacheManager.PBTable.LiveSeasonTable.GetMatchingTarget(competitionInfo.rating_idx);
            if(matchingTarget == (byte)COMPETITION_MATCH_TARGET.BOT)
            {
                matchBattleInfo = CacheManager.PBTable.LiveSeasonTable.GetBotBattleData(_webService.WebSession.NationType, competitionInfo.rating_idx);
            }
            else
            {
                // 매칭 처리 ( 1단계 등급 매칭 )
                List<string> keyList = new List<string>();
                long matchingUser = await RatingMatching(matchKey, _rankServer, competitionInfo, keyList);

                if(matchingTarget == (byte)COMPETITION_MATCH_TARGET.ABOVE_ALL_USER)
                {
                    // 루프로 실행한 등급 매칭 유저가 적을 경우 봇 데이터 적용
                    if (matchingUser <= CacheManager.PBTable.ConstantTable.PvpConst.ranked_matching_rank_minuser)
                    {
                        matchBattleInfo = CacheManager.PBTable.LiveSeasonTable.GetBotBattleData(_webService.WebSession.NationType, competitionInfo.rating_idx);
                    }
                    else if (matchingUser < CacheManager.PBTable.ConstantTable.PvpConst.ranked_matching_rank_maxuser)
                    {
                        // 1단계 매칭 성공
                        List<long> users = _rankServer.GetMatchUsers(keyList);
                        matchBattleInfo = await GetBattleInfoFromUsers(users, _rankServer, gameDB, matchHistory, webSession.TokenInfo.Pcid);
                    }
                }
                else
                {
                    if (matchingUser < CacheManager.PBTable.ConstantTable.PvpConst.ranked_matching_rank_maxuser)
                    {
                        // 1단계 매칭 성공
                        List<long> users = _rankServer.GetMatchUsers(keyList);
                        matchBattleInfo = await GetBattleInfoFromUsers(users, _rankServer, gameDB, matchHistory, webSession.TokenInfo.Pcid);
                    }
                }

                if(matchBattleInfo == null)
                {
                    // 매칭 처리 ( 2단계 팀전력 매칭 )
                    List<long> users = new List<long>();

                    int start = reqData.TeamOverall - CacheManager.PBTable.ConstantTable.PvpConst.ranked_matching_teampower_rng;
                    int end = reqData.TeamOverall + CacheManager.PBTable.ConstantTable.PvpConst.ranked_matching_teampower_rng;

                    foreach (string key in keyList)
                    {
                        users.AddRange(await _rankServer.GetRangeMatchUsers(key, start, end));
                    }

                    // 팀전력 매칭 유저 수가 적을 경우
                    if (users.Count <= CacheManager.PBTable.ConstantTable.PvpConst.ranked_matching_teampower_minuser)
                    {
                        matchBattleInfo = await GetBattleInfoFromUsers(_rankServer.GetMatchUsers(keyList), _rankServer, gameDB, matchHistory, webSession.TokenInfo.Pcid);
                    }
                    else if (users.Count < CacheManager.PBTable.ConstantTable.PvpConst.ranked_matching_teampower_maxuser)
                    {
                        matchBattleInfo = await GetBattleInfoFromUsers(users, _rankServer, gameDB, matchHistory, webSession.TokenInfo.Pcid);
                    }
                    else
                    {
                        // 매칭 처리 ( 3단계 총 전적 매칭 )
                        int myTotalRecord = competitionRecord.win + competitionRecord.draw + competitionRecord.lose;
                        int myRecordCountMin = myTotalRecord - CacheManager.PBTable.ConstantTable.PvpConst.ranked_matching_record_rng;
                        int myRecordCountMax = myTotalRecord + CacheManager.PBTable.ConstantTable.PvpConst.ranked_matching_record_rng;

                        List<string> battleKeyList = new List<string>();

                        foreach (long user in users)
                        {
                            if (matchHistory.Contains(user) == true)
                            {
                                continue;
                            }
                            if (user == webSession.TokenInfo.Pcid)
                            {
                                continue;
                            }

                            battleKeyList.Add(CacheManager.PBTable.LiveSeasonTable.GetBattleKey(user));
                        }

                        IDictionary<string, BattleInfo> getUsers = await _rankServer.GetData<BattleInfo>(battleKeyList);

                        var findUsers = getUsers.Where(x =>
                        {
                            int totalRecord = x.Value.win + x.Value.draw + x.Value.lose;
                            return myRecordCountMin < totalRecord && totalRecord < myRecordCountMax;
                        }).ToList();


                        // 유저가 적을 경우 팀전력 매칭 유저에서 가져옴
                        if (findUsers.Count <= CacheManager.PBTable.ConstantTable.PvpConst.ranked_matching_record_minuser)
                        {
                            matchBattleInfo = await GetBattleInfoFromUsers(users, _rankServer, gameDB, matchHistory, webSession.TokenInfo.Pcid);
                        }
                        else if (findUsers.Count < CacheManager.PBTable.ConstantTable.PvpConst.ranked_matching_record_maxuser)
                        {
                            findUsers.ShuffleForSelectedCount(findUsers.Count / 2);
                            matchBattleInfo = findUsers.ElementAt(RandomManager.Instance.GetIndex(RANDOM_TYPE.GLOBAL, findUsers.Count)).Value;
                        }
                        else
                        {
                            // 매칭 처리 ( 4단계 승률 매칭 )
                            int myWinRate = (myTotalRecord / myRecordCountMax) * 100;
                            int myWinRateMin = myWinRate - CacheManager.PBTable.ConstantTable.PvpConst.ranked_matching_winrate_rng;
                            int myWinRateMax = myWinRate + CacheManager.PBTable.ConstantTable.PvpConst.ranked_matching_winrate_rng;

                            var winrateUsers = findUsers.Where(x =>
                            {
                                int winRate = ((x.Value.win + x.Value.draw + x.Value.lose) / x.Value.win) * 100;
                                return myWinRateMin < winRate && winRate < myWinRateMax;

                            }).ToList();

                            // 유저가 적을 경우 총전적 조건에서 가져옴
                            if (findUsers.Count <= CacheManager.PBTable.ConstantTable.PvpConst.ranked_matching_winrate_minuser)
                            {
                                findUsers.ShuffleForSelectedCount(findUsers.Count / 2);
                                matchBattleInfo = findUsers.ElementAt(RandomManager.Instance.GetIndex(RANDOM_TYPE.GLOBAL, findUsers.Count)).Value;
                            }
                            else
                            {
                                winrateUsers.ShuffleForSelectedCount(winrateUsers.Count / 2);
                                matchBattleInfo = winrateUsers.ElementAt(RandomManager.Instance.GetIndex(RANDOM_TYPE.GLOBAL, winrateUsers.Count)).Value;
                            }
                        }
                    }
                }
            }

            if (matchBattleInfo == null)
            {
                return _webService.End(ErrorCode.ERROR_FAILED_MATCHING);
            }
            bool isExistedKey = await _rankServer.IsExistKey(matchKey);

            // 내 등급 정보 저장
            double myScore = await _rankServer.GetScore(matchKey, webSession.TokenInfo.Pcid);

            if (myScore == 0)
            {
                await _rankServer.SetScore(matchKey, webSession.TokenInfo.Pcid, reqData.TeamOverall);
            }
            else if (myScore != reqData.TeamOverall) // 오버롤이 변경됬을 경우
            {
                double value = reqData.TeamOverall - myScore;
                if (value > 0)
                {
                    double result = await _rankServer.IncreaseScore(matchKey, webSession.TokenInfo.Pcid, value);
                }
                else if (value < 0)
                {
                    double result = await _rankServer.DecreaseScore(matchKey, webSession.TokenInfo.Pcid, -value);
                }
            }

            // 해당 키 Expired Time 저장
            if (isExistedKey == false)
            {
                int matchExpiredDate = ServerUtils.GetConfigValue<int>(_config.GetSection("GlobalConfig"), "RedisMatchExpiredTime");
                await _rankServer.SetExpiredTime(matchKey, new TimeSpan(matchExpiredDate, 0, 0, 0));
            }

            // 대전 히스토리 저장
            matchHistory.Enqueue(webSession.TokenInfo.Pcid);
            if (matchHistory.Count > CacheManager.PBTable.ConstantTable.PvpConst.ranked_matching_prevusercooltime)
            {
                matchHistory.Dequeue();
            }

            string matchHistoryStr = JsonConvert.SerializeObject(matchHistory);

            // 세션 배틀 키 지정
            string battleKey = KeyGenerator.Instance.GetIncrementKey(GAME_KEY_TYPE.COMPETITION_GAME);
            _webService.WebSession.BattleKey = battleKey;

            // 정보 업데이트
            if (gameDB.USP_GS_GM_LIVESEASON_COMPETITION_START(webSession.TokenInfo.Pcid, battleKey, matchHistoryStr) == false)
            {
                return _webService.End(ErrorCode.ERROR_DB, "USP_GS_GM_LIVESEASON_RATINGBATTLE_START");
            }

            // 응답 처리
            resData.BattleInfo = matchBattleInfo;

            return _webService.End();
        }

        private async Task<long> RatingMatching(string rankKey, RankServer rankServer, CompetitionInfo competitionInfo, List<string> matchkeyList)
        {
            long totalCount = 0;

            totalCount += await rankServer.GetRankTotalCount(rankKey);
            matchkeyList.Add(rankKey);

            for (int offset = 1; offset <= CacheManager.PBTable.ConstantTable.PvpConst.ranked_matching_rank_rng; ++offset)
            {
                // 등급 매칭 유저가 적을 경우
                if (totalCount < CacheManager.PBTable.ConstantTable.PvpConst.ranked_matching_rank_minuser)
                {
                    int nextIdx = CacheManager.PBTable.LiveSeasonTable.NextRating(competitionInfo.rating_idx, offset);
                    int beforeIdx = CacheManager.PBTable.LiveSeasonTable.BeforeRating(competitionInfo.rating_idx, offset);

                    // 상위 등급 체크
                    if (nextIdx != competitionInfo.rating_idx)
                    {
                        string nextRankKey = CacheManager.PBTable.LiveSeasonTable.GetMatchKey(competitionInfo.season_idx,nextIdx);
                        totalCount += await _rankServer.GetRankTotalCount(nextRankKey);
                        matchkeyList.Add(nextRankKey);
                    }
                    if (totalCount < CacheManager.PBTable.ConstantTable.PvpConst.ranked_matching_rank_minuser)
                    {
                        // 하위 등급 체크
                        if (beforeIdx != competitionInfo.rating_idx)
                        {
                            string beforeRankKey = CacheManager.PBTable.LiveSeasonTable.GetMatchKey(competitionInfo.season_idx,beforeIdx);
                            totalCount += await _rankServer.GetRankTotalCount(beforeRankKey);
                            matchkeyList.Add(beforeRankKey);
                        }
                    }
                }
            }

            return totalCount;
        }

        private async Task<BattleInfo> GetBattleInfoFromUsers(List<long> users, RankServer rankServer, GameDB gameDB, Queue<long> history, long myPcId)
        {
            users.Remove(myPcId);
            users.RemoveAll(x => history.Contains(x));

            int index = RandomManager.Instance.GetIndex(RANDOM_TYPE.GLOBAL, users.Count);
            long user = users[index];

            _logger.LogInformation("Find Battle User - pcId:{0}", user);

            string enemyKey = CacheManager.PBTable.LiveSeasonTable.GetBattleKey(user);
            BattleInfo matchBattleInfo = await rankServer.GetData<BattleInfo>(enemyKey);
            if (matchBattleInfo == null)
            {
                _logger.LogWarning("Not Find Battle User From RankServer - pcId:{0}", user);

                if (GetMatchDataFromDB(user, gameDB, out matchBattleInfo) == false)
                {
                    return null;
                }
            }

            return matchBattleInfo;
        }

        private bool GetMatchDataFromDB(long user, GameDB gameDB, out BattleInfo battleInfo)
        {
            battleInfo = null;

            // DB 에서 해당 정보 가져옴
            DataSet enemyDataSet = gameDB.USP_GM_BATTLE_INFO_R(user);
            if (enemyDataSet == null)
            {
                return false;
            }

            DataSetWrapper enemyDataSetWrapper = new DataSetWrapper(enemyDataSet);
            if (enemyDataSetWrapper.GetTableCount() != 4 )
            {
                return false;
            }

            string nickName = enemyDataSetWrapper.GetValue<string>(0, "nick_name");
            int teamIdx = enemyDataSetWrapper.GetValue<int>(0, "team_idx");
            byte nationType = enemyDataSetWrapper.GetValue<byte>(0, "nation_type");
            List<BattlePlayer> playerList = enemyDataSetWrapper.GetObjectList<BattlePlayer>(1);
            List<BattleCoach> coachList = enemyDataSetWrapper.GetObjectList<BattleCoach>(2);
            int win = enemyDataSetWrapper.GetValue<int>(3, "win");
            int draw = enemyDataSetWrapper.GetValue<int>(3, "draw");
            int lose = enemyDataSetWrapper.GetValue<int>(3, "lose");

            battleInfo = new BattleInfo
            {
                nick_name = nickName,
                team_Idx = teamIdx,
                nation_type = nationType,
                win = win,
                draw = draw,
                lose = lose,
                player_list = playerList,
                coach_list = coachList
            };

            return true;
        }
    }
}
