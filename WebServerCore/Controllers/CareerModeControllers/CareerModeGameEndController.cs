using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MsgPack.Serialization;
using System.Collections.Generic;
using System.Data;
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
using System.Text;
using ApiWebServer.Common;
using Newtonsoft.Json;

namespace ApiWebServer.Controllers.CareerModeControllers
{
    [Route("api/CareerMode/[controller]")]
    [ApiController]
    public class CareerModeGameEndController : SessionContoller<ReqCareerModeGameEnd, ResCareerModeGameEnd>
    {
        private static byte eventDeleteAll = 1;
        private static byte eventDeleteNotSelect = 2;

        public CareerModeGameEndController(
            ILogger<CareerModeGameEndController> logger,
            IConfiguration config,
            IWebService<ReqCareerModeGameEnd, ResCareerModeGameEnd> webService,
            IDBService dbService)
            : base(logger, config, webService, dbService)
        {
        }

        [HttpPost]
        [ApiExplorerSettings(GroupName = "client")]
        [SwaggerExtend("커리어모드 게임 종료", typeof(CareerModeGameEndPacket))]
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

            if( reqData.PlayingPitcherList == null || reqData.PlayingPitcherList.Count == 0 ||
                reqData.PlayingBatterList == null || reqData.PlayingBatterList.Count == 0)
            {
                return _webService.End(ErrorCode.ERROR_INVALID_PARAM);
            }

            StringBuilder playerSb = new StringBuilder();
            for (int i = 0; i < reqData.PlayingPitcherList.Count; ++i)
                playerSb.AppendFormat("{0},", reqData.PlayingPitcherList[i].account_player_idx);
            for (int i = 0; i < reqData.PlayingBatterList.Count; ++i)
                playerSb.AppendFormat("{0},", reqData.PlayingBatterList[i]);

            playerSb.Remove(playerSb.Length - 1, 1);


            // 기본 정보 조회 ( 미션 및 업적도 가져와야 함... )
            DataSet dataSet = gameDB.USP_GS_GM_CAREERMODE_GAME_END_R(webSession.TokenInfo.Pcid, reqData.MvpPlayer, playerSb.ToString());
            if (dataSet == null)
            {
                return _webService.End(ErrorCode.ERROR_DB, "USP_GS_GM_CAREERMODE_GAME_END_R");
            }

            DataSetWrapper dataSetWrapper = new DataSetWrapper(dataSet);
            CareerModeInfo careerModeInfo = dataSetWrapper.GetObject<CareerModeInfo>(0);
            AccountGame accountGameInfo = dataSetWrapper.GetObject<AccountGame>(1);
            bool isMvpMyTeam = dataSetWrapper.GetValue<bool>(2, "is_mvp_my_team");
            List<CareerModeMission> careerModeMissions = dataSetWrapper.GetObjectList<CareerModeMission>( 3 );

            List<PlayerCareerPlayingInfo> playingPlayerInfo = dataSetWrapper.GetObjectList<PlayerCareerPlayingInfo>(4);
            string battleKey = dataSetWrapper.GetValue<string>(5, "battle_key");
            bool isSimulItemUse = dataSetWrapper.GetValue<bool>(5, "is_simul_item");

            List<PlayerCareerPlayingInfo> updatePlayerInfo = new List<PlayerCareerPlayingInfo>();   //db에 업데이트될 선수 정보
            List<PlayerCareerInjuryInfo> injuryPlayerInfo = new List<PlayerCareerInjuryInfo>();         //클라로 보내줄 선수 부상정보
            careerModeInfo.match_type = reqData.MatchType;
            careerModeInfo.half_type = reqData.HalfYear;

            // 현재 커리어 모드 검증 
            if (reqData.CareerNo != careerModeInfo.career_no ||
                reqData.DegreeNo != careerModeInfo.degree_no ||
                reqData.MatchGroup != careerModeInfo.match_group )
            {
                return _webService.End(ErrorCode.ERROR_NOT_MATCHING_INFO );
            }
            else if( careerModeInfo.springcamp_step != ( byte )SPRING_CAMP_STEP.FINISH )
            {
                return _webService.End( ErrorCode.ERROR_INVALID_SPRINGCMAP_STEP );
            }
            else if( careerModeInfo.specialtraining_step != ( byte )SPECIAL_TRAINING_STEP.NULL )
            {
                return _webService.End( ErrorCode.ERROR_INVALID_SPECIALTRAINING_STEP );
            }
            else if(playingPlayerInfo.Count != reqData.PlayingPitcherList.Count + reqData.PlayingBatterList.Count)
            {
                return _webService.End(ErrorCode.ERROR_INVALID_PLAYINGPLAYER_INFO);
            }

            // 배틀 키 검증
            if (webSession.BattleKey == null || webSession.BattleKey != battleKey)
            {
                return _webService.End(ErrorCode.ERROR_INVALID_BATTLE_KEY);
            }

            // 시뮬레이션이 아닐 경우 배틀 키로 유효한 시간 체크
            if(isSimulItemUse == false)
            {
                // 대전 키로 유효한 시간 체크
                int battleValidTime = ServerUtils.GetConfigValue<int>(_config.GetSection("Config"), "BattleValidTime");
                if (KeyGenerator.Instance.ValidateKey(GAME_KEY_TYPE.CAREER_MODE_GAME, webSession.BattleKey, battleValidTime) == false)
                {
                    return _webService.End(ErrorCode.ERROR_INVALID_BATTLE_KEY);
                }
            }

            // 미션 & 업적 처리 생성
            MissionAchievement missionAchievement = new MissionAchievement(webSession.TokenInfo.Pcid, gameDB, _webService.WebPacket.ResHeader);
            missionAchievement.Input(webSession.MissionList, webSession.AchievementList);

            if (reqData.GameResult == (byte)GAME_RESULT.WIN)
            {
                missionAchievement.AddAction(ActionTypeDefine.CareerModeWinCount, 1);
            }

            byte deleteEventFlag = 0;           //1:모든이벤트 삭제 2:석택안한이벤트만 삭제

            List<CareerModeCycleEventInfo> newCycleEventList = null;

            //체력 처리
            for (int i = 0; i < reqData.PlayingPitcherList.Count; ++i)
            {
                PlayerCareerPlayingInfo player = playingPlayerInfo.Find(x => x.account_player_idx == reqData.PlayingPitcherList[i].account_player_idx);

                if(player == null)
                    return _webService.End(ErrorCode.ERROR_NOT_PLAYER);

                //업뎃 목록에 선수의 체력 갱신
                updatePlayerInfo.Add(new PlayerCareerPlayingInfo()
                {
                    account_player_idx = player.account_player_idx,
                    injury_idx = player.injury_idx,
                    injury_period = player.injury_period,
                    injury_add_ratio = player.injury_add_ratio,
                    injury_cure_no = player.injury_cure_no,
                    health_game_no = careerModeInfo.game_no,
                    player_health = reqData.PlayingPitcherList[i].player_health
                });
            }

            // 페넌트레이스
            if (reqData.MatchGroup == (byte)SEASON_MATCH_GROUP.PENNANTRACE)
            {
                if (careerModeInfo.finish_match_group != (byte)SEASON_MATCH_GROUP.NONE)
                    return _webService.End(ErrorCode.ERROR_NOT_PENNANTRACE_SEASON);

                if (reqData.FinishMatchGroup == 1)
                {
                    careerModeInfo.finish_match_group = (byte)SEASON_MATCH_GROUP.PENNANTRACE;

                    careerModeInfo.teammood = (short)CacheManager.PBTable.CareerModeTable.ManagementConfig.teammood_default;
                    careerModeInfo.event_flag = (byte)CYCLE_EVENT_FLAG.NOT_CYCLE;

                }
                else
                {
                    careerModeInfo.specialtraining_step = GetSpecialTrainingStep(careerModeInfo.country_type, careerModeInfo.game_no);

                    //패넌트레이트 일때만 부상 체크한다.
                    CacheManager.PBTable.CareerModeTable.CheckChainOccurInjury(playingPlayerInfo, ref careerModeInfo, ref updatePlayerInfo, ref injuryPlayerInfo);
                    CacheManager.PBTable.CareerModeTable.CheckNewOccurInjury(playingPlayerInfo, ref careerModeInfo, ref updatePlayerInfo, ref injuryPlayerInfo);

                    careerModeInfo.teammood += CacheManager.PBTable.CareerModeTable.GetAddTeamMood(reqData.GameResult);
                    //관리주기 체크
                    if (CacheManager.PBTable.CareerModeTable.IsCycleEvent(careerModeInfo.country_type, careerModeInfo.game_no) == true)
                    {
                        newCycleEventList = CacheManager.PBTable.CareerModeTable.GetNewCycleEventList(careerModeInfo.teammood, out int[] searchCnt);

                        deleteEventFlag = eventDeleteAll;
                        careerModeInfo.event_flag = (byte)CYCLE_EVENT_FLAG.NEW_CYCLE_NOT_EVENT;          //신규이벤트 없음
                        careerModeInfo.teammood = (short)CacheManager.PBTable.CareerModeTable.ManagementConfig.teammood_default;

                        //이벤트 발생했다면
                        if (newCycleEventList != null)
                        {
                            careerModeInfo.event_flag = (byte)CYCLE_EVENT_FLAG.NEW_CYCLE_NEW_EVENT;      //신규 이벤트 있음

                            if(searchCnt[0] > 0 || searchCnt[1] > 0 || searchCnt[2] > 0)    //idx 0:전체, 1:타자전체, 2:투수전체
                            {
                                DataSet searchDataSet = gameDB.USP_GS_GM_CAREERMODE_GAME_END_RANDOM_PLAYER(webSession.TokenInfo.Pcid, searchCnt);
                                if (searchDataSet == null)
                                {
                                    return _webService.End(ErrorCode.ERROR_DB, "USP_GS_GM_CAREERMODE_GAME_END_RANDOM_PLAYER");
                                }

                                if(searchCnt[0] != searchDataSet.Tables[0].Rows.Count || searchCnt[1] != searchDataSet.Tables[1].Rows.Count || searchCnt[2] != searchDataSet.Tables[2].Rows.Count)
                                {
                                    return _webService.End(ErrorCode.ERROR_NOT_PLAYER, "USP_GS_GM_CAREERMODE_GAME_END_RANDOM_PLAYER");
                                }

                                int[] searchSeek = { 0, 0, 0 };

                                foreach (CareerModeCycleEventInfo eventInfo in newCycleEventList)
                                {
                                    if(eventInfo.target_player > 0)
                                    {
                                        int idx = (int)eventInfo.target_player - 1;
                                        eventInfo.target_player = (long)searchDataSet.Tables[idx].Rows[searchSeek[idx]++]["account_player_idx"];
                                    }
                                }
                            }
                        }
                    }
                    else if (careerModeInfo.event_flag > (byte)CYCLE_EVENT_FLAG.NOT_CYCLE)
                    {
                        deleteEventFlag = eventDeleteNotSelect;
                        careerModeInfo.event_flag = (byte)CYCLE_EVENT_FLAG.NOT_CYCLE;
                    }

                }

            }
            // 포스트 시즌
            else if (reqData.MatchGroup == (byte)SEASON_MATCH_GROUP.POST_SEASON)
            {
                if (careerModeInfo.finish_match_group != (byte)SEASON_MATCH_GROUP.PENNANTRACE)
                    return _webService.End(ErrorCode.ERROR_NOT_POST_SEASON);

                if (reqData.FinishMatchGroup == 1)
                    careerModeInfo.finish_match_group = (byte)SEASON_MATCH_GROUP.POST_SEASON;

                careerModeInfo.event_flag = (byte)CYCLE_EVENT_FLAG.NOT_CYCLE;
            }

            // 기록 처리
            string teamIdx = careerModeInfo.team_idx.ToString();
            if ( reqData.GameRecord.ContainsKey( teamIdx ) == false )
            {
                return _webService.End( ErrorCode.ERROR_NOT_FOUND_CAREER_TEAM_RECORD );
            }

            string teamRecord = reqData.GameRecord[ teamIdx ].team;
            byte[] gameRecord = MessagePackSerializer.Get<Dictionary<string, CareerModeGameRecord>>().PackSingleObject( reqData.GameRecord );
            string jsonRecord = JsonConvert.SerializeObject(reqData.GameRecord);

            _logger.LogInformation("temp serialize size - msgpack:{0}, json:{1}", gameRecord.Length, jsonRecord.Length);

            // 감독 기록 미션 업데이트
            List<CareerModeMission> updateMissions = CacheManager.PBTable.CareerModeTable.GetUpdatedCareerRecordMission( careerModeMissions, reqData.MatchGroup, reqData.CommendCount, reqData.GameResult, teamRecord);

            // 스테이지 보상 정보 가져옴
            if (false == CacheManager.PBTable.CareerModeTable.StageRewardInfo(careerModeInfo.mode_level, reqData.GameResult, isMvpMyTeam, out List<GameRewardInfo> stageRewardList, out GameRewardInfo mvpReward, out int addManagerExp))
            {
                return _webService.End(ErrorCode.ERROR_NOT_FOUND_CAREER_DATA, $"difficulty:{careerModeInfo.mode_level}, gameResult:{reqData.GameResult}");
            }

            ConsumeReward consumeReward;

            if (isSimulItemUse == true)
            {
                consumeReward = new ConsumeReward(webSession.TokenInfo.Pcid, gameDB, CONSUME_REWARD_TYPE.CONSUMEREWARD, true);
                consumeReward.AddConsume(new GameRewardInfo((byte)REWARD_TYPE.NORMAL_ITEM, CacheManager.PBTable.ItemTable.itemIdxCareermodeSimulration, 1));
            }
            else
            {
                consumeReward = new ConsumeReward(webSession.TokenInfo.Pcid, gameDB, CONSUME_REWARD_TYPE.REWARD, true);
            }

            // 보상 정보 처리
            consumeReward.AddReward(stageRewardList);
            if (mvpReward != null)
                consumeReward.AddReward(mvpReward);

            ErrorCode rewardResult = consumeReward.Run(ref accountGameInfo, false);
            if (rewardResult != ErrorCode.SUCCESS)
            {
                return _webService.End(rewardResult);
            }

            // 경험치 처리
            LevelUp levelUp = new LevelUp(accountGameInfo);
            levelUp.AddExp(addManagerExp, out bool isLevelUp);

            // 레벨업 시 처리
            if (isLevelUp)
            {

            }

            missionAchievement.AddAction(ActionTypeDefine.CareerModeRunCount, 1);

            // 결과 저장
            if (gameDB.USP_GS_GM_CAREERMODE_GAME_END(webSession.TokenInfo.Pcid, reqData, careerModeInfo, updatePlayerInfo, accountGameInfo, consumeReward.GetUpdateItemList(),
                updateMissions, teamRecord, gameRecord, deleteEventFlag, newCycleEventList) == false)
            {
                return _webService.End(ErrorCode.ERROR_DB, "USP_GS_GM_CAREERMODE_GAME_END");
            }

            // 미션 업적 정보 저장
            if (missionAchievement.Update() == false)
            {
                return _webService.End(ErrorCode.ERROR_FAILED_UPDATE_DATA);
            }

            webSession.BattleKey = null;

            resData.AddManagerExp = addManagerExp;
            resData.AddManagerBonusExp = 0;

            resData.ManagerExp = accountGameInfo.user_exp;
            resData.ManagerLv = accountGameInfo.user_lv;

            resData.RewardInfo = stageRewardList;
            resData.MvpReward = mvpReward;

            resData.ResultAccountCurrency = accountGameInfo;
            resData.UpdateItemInfo = consumeReward.GetUpdateItemList();

            resData.InjuryPlayerInfo = injuryPlayerInfo;
            resData.CareerModeMissions = updateMissions;

            return _webService.End();
        }

        private byte GetSpecialTrainingStep(byte countryType, int gameNo)
        {
            int middleGameNo = -1;
            if (countryType == (byte)NATION_LEAGUE_TYPE.KBO)
                middleGameNo = CacheManager.PBTable.ConstantTable.Const.schedule_half_kbo;
            else if (countryType == (byte)NATION_LEAGUE_TYPE.MLB)
                middleGameNo = CacheManager.PBTable.ConstantTable.Const.schedule_half_mlb;
            else if (countryType == (byte)NATION_LEAGUE_TYPE.NPB)
                middleGameNo = CacheManager.PBTable.ConstantTable.Const.schedule_half_npb;
            else if (countryType == (byte)NATION_LEAGUE_TYPE.CPB)
                middleGameNo = CacheManager.PBTable.ConstantTable.Const.schedule_half_cpbl;

            if (middleGameNo == gameNo)
                return (byte)SPECIAL_TRAINING_STEP.STEP_MIDDLE;
            else
                return (byte)SPECIAL_TRAINING_STEP.NULL;
        }
    }
}
