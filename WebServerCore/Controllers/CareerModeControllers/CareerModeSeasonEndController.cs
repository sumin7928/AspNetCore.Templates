using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Data;
using System.Linq;
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

namespace ApiWebServer.Controllers.CareerModeControllers
{
    [Route("api/CareerMode/[controller]")]
    [ApiController]
    public class CareerModeSeasonEndController : SessionContoller<ReqCareerModeSeasonEnd, ResCareerModeSeasonEnd>
    {
        public CareerModeSeasonEndController(
            ILogger<CareerModeSeasonEndController> logger,
            IConfiguration config,
            IWebService<ReqCareerModeSeasonEnd, ResCareerModeSeasonEnd> webService,
            IDBService dbService)
            : base(logger, config, webService, dbService)
        {
        }

        [HttpPost]
        [ApiExplorerSettings(GroupName = "client")]
        [SwaggerExtend("커리어모드 시즌 종료", typeof(CareerModeSeasonEndPacket))]
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
            List<int> myAwardTypeRewardKeys = new List<int>();
            bool isCompleteContractMission = false;

            // 기본 정보 조회
            DataSet dataSet = gameDB.USP_GS_GM_CAREERMODE_SEASON_END_R(webSession.TokenInfo.Pcid, reqData.CareerNo);
            if (dataSet == null)
            {
                return _webService.End(ErrorCode.ERROR_DB, "USP_GS_GM_CAREERMODE_SEASON_END_R");
            }

            DataSetWrapper dataSetWrapper = new DataSetWrapper(dataSet);
            CareerModeInfo careerModeInfo = dataSetWrapper.GetObject<CareerModeInfo>(0);
            AccountGame accountGameInfo = dataSetWrapper.GetObject<AccountGame>(1);
            List<CareerModeMission> missionList = dataSetWrapper.GetObjectList<CareerModeMission>(2);
            CareerModeHistory history = dataSetWrapper.GetObject<CareerModeHistory>(3);

            // 현재 커리어 모드 검증
            if (reqData.CareerNo != careerModeInfo.career_no ||
                reqData.MatchGroup != careerModeInfo.match_group ||
                reqData.TeamRank != careerModeInfo.now_rank)
            {
                return _webService.End(ErrorCode.ERROR_NOT_MATCHING_INFO);
            }

            if (reqData.FinishAllSeason == 0 && reqData.MatchGroup != (byte)SEASON_MATCH_GROUP.PENNANTRACE)
            {
                return _webService.End(ErrorCode.ERROR_NOT_MATCHING_INFO);
            }

            if (careerModeInfo.country_type == (byte)NATION_LEAGUE_TYPE.CPB && careerModeInfo.half_type != (byte)SEASON_HALF_YEAR_TYPE.SECOND)
            {
                return _webService.End(ErrorCode.ERROR_NOT_MATCHING_INFO);
            }

            // 국가별 차수 체크 필요.. (임시 제거)
            //if (careerModeInfo.country_type == (byte)NATION_LEAGUE_TYPE.KBO)
            //{
            //    if( careerModeInfo.degree_no < CareerModeDefine.MaxPennantraceDegreeInKBO )
            //    {
            //        return _webService.End(ErrorCode.ERROR_INVALID_DEGREE_NO);
            //    }
            //}

            List<CareerModeMission> updateMissions = null;
            List<CareerModeMission> newMissions = null;

            // 패넌트레이스 시즌 
            if (reqData.MatchGroup == (byte)SEASON_MATCH_GROUP.PENNANTRACE)
            {
                if (careerModeInfo.finish_match_group != (byte)SEASON_MATCH_GROUP.PENNANTRACE)
                {
                    return _webService.End(ErrorCode.ERROR_NOT_MATCHING_INFO, "invalidate career finish match group in pennantrace");
                }

                if (reqData.ListSeasonAwardPlayers == null || reqData.ListSeasonAwardPlayers.Count != (int)AWARD_TYPE.MAX)
                {
                    return _webService.End(ErrorCode.ERROR_NOT_MATCHING_INFO, "invalidate last season award players");
                }

                // 내 선수만 가져옴
                Dictionary<int, CareerModeSeasonAwardPlayer> myAwardPlayers = ConvertMyAwardPlayers(reqData.ListSeasonAwardPlayers);
                byte awardPlayerCount = 0;
                if (myAwardPlayers != null)
                {
                    // 내팀 기록정보 조회
                    DataSet recordDataSet = gameDB.USP_GS_GM_CAREERMODE_SEASON_END_RECORD_INFO(webSession.TokenInfo.Pcid);
                    if (recordDataSet == null)
                    {
                        return _webService.End(ErrorCode.ERROR_DB, "USP_GS_GM_CAREERMODE_SEASON_END_RECORD_INFO");
                    }

                    var recordDataSetWrapper = new DataSetWrapper(recordDataSet);
                    var peGame = recordDataSetWrapper.GetValue<byte[]>(0, "pe_game");
                    if (peGame == null)
                    {
                        return _webService.End(ErrorCode.ERROR_DB_ROW_COUNT, "USP_GS_GM_CAREERMODE_SEASON_END_RECORD_INFO");
                    }
                    var gameRecord = MsgPack.Serialization.MessagePackSerializer.Get<Dictionary<string, CareerModeGameRecord>>().UnpackSingleObject(peGame);
                    if (gameRecord.ContainsKey(careerModeInfo.team_idx.ToString()) == false)
                    {
                        return _webService.End(ErrorCode.ERROR_NOT_FOUND_TEAM_RECORD);
                    }
                    var record = gameRecord[careerModeInfo.team_idx.ToString()];

                    // 어워드 선수 정보 검증
                    int gameCount = careerModeInfo.degree_no - 1;
                    if (IsValidAwardPlayers(myAwardPlayers, record, gameCount) == false)
                    {
                        return _webService.End(ErrorCode.ERROR_NOT_MATCHING_MVP_RECORD);
                    }

                    // 수상 정보 보상 키로 변환
                    foreach (var myPlayers in myAwardPlayers)
                    {
                        myAwardTypeRewardKeys.Add(myPlayers.Key + 1);
                    }

                    awardPlayerCount = (byte)myAwardPlayers.Count;
                }

                careerModeInfo.last_rank = reqData.TotalRank;
                bool isEnterancePost = false;
                if (reqData.FinishAllSeason == 0)
                {
                    isEnterancePost = true;
                }

                updateMissions = CacheManager.PBTable.CareerModeTable.GetUpdatedPennantraceSeasonMission(missionList, reqData.TeamRank, awardPlayerCount, isEnterancePost);

                // 새로 포스트 시즌 미션 저장
                newMissions = new List<CareerModeMission>();
                List<int> postMissionList = CacheManager.PBTable.CareerModeTable.GetPostMissionList();
                foreach (int mission_idx in postMissionList)
                {
                    newMissions.Add(new CareerModeMission() { mission_idx = mission_idx });
                }
            }
            // 포스트 시즌
            else
            {
                if (careerModeInfo.finish_match_group != (byte)SEASON_MATCH_GROUP.POST_SEASON)
                {
                    return _webService.End(ErrorCode.ERROR_NOT_MATCHING_INFO, "invalidate career finish match group in post");
                }

                updateMissions = CacheManager.PBTable.CareerModeTable.GetUpdatedPostSeasonMission(missionList, reqData.TeamRank);
            }

            // 시즌 보상 정보 가져옴
            if (false == CacheManager.PBTable.CareerModeTable.SeasonRewardInfo(careerModeInfo, reqData.TeamRank, myAwardTypeRewardKeys,
                out List<GameRewardInfo> rankRewardList, out List<GameRewardInfo> mvpRewardList))
            {
                return _webService.End(ErrorCode.ERROR_NOT_FOUND_CAREER_DATA, $"countryType:{careerModeInfo.country_type}, modeLevel:{careerModeInfo.mode_level}, matchGroup:{careerModeInfo.match_group}, teamRank{reqData.TeamRank}");
            }

            // 미션 보상 및 추천팀 보상
            List<GameRewardInfo> missionRewardList = CacheManager.PBTable.CareerModeTable.GetMissionRewardInfo(missionList, out isCompleteContractMission);
            List<GameRewardInfo> recommendTeamReward = null;

            // 시즌 전체가 끝났을 경우
            if (reqData.FinishAllSeason > 0)
            {
                careerModeInfo.match_group = (byte)SEASON_MATCH_GROUP.FINISHED;
                careerModeInfo.specialtraining_step = (byte)SPECIAL_TRAINING_STEP.NULL;
                if (isCompleteContractMission)
                {
                    careerModeInfo.previous_contract = (byte)CONTRACT_TYPE.STAND_BY_CONTRACT;
                }
                else
                {
                    careerModeInfo.previous_contract = (byte)CONTRACT_TYPE.STAND_BY_FAILED;
                }

                // 추천팀 추가 보상
                if (careerModeInfo.recommend_reward_idx > 0)
                {
                    recommendTeamReward = CacheManager.PBTable.CareerModeTable.GetRecommendTeamRewrad(careerModeInfo.recommend_reward_idx);
                }
            }
            else
            {
                careerModeInfo.match_group = (byte)SEASON_MATCH_GROUP.POST_SEASON;
                //임시처리 (2차 특별훈련 안되도록 수정)
                //careerModeInfo.specialtraining_step = (byte)SPECIAL_TRAINING_STEP.STEP_LAST;
            }

            // 보상 정보 처리
            ConsumeReward reward = new ConsumeReward(webSession.TokenInfo.Pcid, gameDB, CONSUME_REWARD_TYPE.REWARD, false);
            reward.AddReward(rankRewardList);
            reward.AddReward(mvpRewardList);
            reward.AddReward(missionRewardList);
            reward.AddReward(recommendTeamReward);
            ErrorCode rewardResult = reward.Run(ref accountGameInfo, false);
            if (rewardResult != ErrorCode.SUCCESS)
            {
                return _webService.End(rewardResult);
            }

            // 결과 저장
            if (gameDB.USP_GS_GM_CAREERMODE_SEASON_END(webSession.TokenInfo.Pcid, careerModeInfo, accountGameInfo, reward.GetUpdateItemList(), updateMissions, newMissions) == false)
            {
                return _webService.End(ErrorCode.ERROR_DB);
            }

            resData.IsCompleteContractMission = isCompleteContractMission;
            resData.MissionRewardInfo = missionRewardList;
            resData.RecommendTeamRewardInfo = recommendTeamReward;
            resData.RankRewardInfo = rankRewardList;
            resData.MvpRewardInfo = mvpRewardList;
            resData.ResultAccountCurrency = accountGameInfo;
            resData.UpdateItemInfo = reward.GetUpdateItemList();
            resData.UpdateMissions = updateMissions;
            resData.NewMissions = newMissions;

            return _webService.End();
        }

        private Dictionary<int, CareerModeSeasonAwardPlayer> ConvertMyAwardPlayers(List<CareerModeSeasonAwardPlayer> requestPlayers)
        {
            Dictionary<int, CareerModeSeasonAwardPlayer> myPlayers = null;

            for (int i = 0; i < requestPlayers.Count; ++i)
            {
                if (requestPlayers[i].is_myteam == 1)
                {
                    if (myPlayers == null)
                    {
                        myPlayers = new Dictionary<int, CareerModeSeasonAwardPlayer>();
                    }

                    myPlayers.Add(i, requestPlayers[i]);
                }
            }

            return myPlayers;
        }

        private bool IsValidAwardPlayers(Dictionary<int, CareerModeSeasonAwardPlayer> myAwardPlayers, CareerModeGameRecord record, int GameCount)
        {
            // 타자 일경우
            if (myAwardPlayers.ContainsKey((int)AWARD_TYPE.HR) ||
                myAwardPlayers.ContainsKey((int)AWARD_TYPE.H) ||
                myAwardPlayers.ContainsKey((int)AWARD_TYPE.RBI) ||
                myAwardPlayers.ContainsKey((int)AWARD_TYPE.SB) ||
                myAwardPlayers.ContainsKey((int)AWARD_TYPE.AVG))
            {
                List<string> resultPlayers = record.batter.Where(x =>
               {
                   string[] records = x.Value.Split(',');
                   int pa = int.Parse(records[(int)BATTER_RECORD_TYPE.PA]);
                   int hr = int.Parse(records[(int)BATTER_RECORD_TYPE.HR]);
                   int h = int.Parse(records[(int)BATTER_RECORD_TYPE.H]);
                   int rbi = int.Parse(records[(int)BATTER_RECORD_TYPE.RBI]);
                   int sb = int.Parse(records[(int)BATTER_RECORD_TYPE.SB]);
                   int avg = int.Parse(records[(int)BATTER_RECORD_TYPE.AVG]);

                   if (pa < (int)(GameCount * 3.1f))
                   {
                       return false;
                   }

                   if (myAwardPlayers.ContainsKey((int)AWARD_TYPE.HR))
                   {
                       if (myAwardPlayers[(int)AWARD_TYPE.HR].award_val < hr)
                       {
                           _logger.LogError("Award record error - HR - my_val:{0}, target_val:{1}", myAwardPlayers[(int)AWARD_TYPE.HR].award_val, hr);
                           return true;
                       }
                   }
                   if (myAwardPlayers.ContainsKey((int)AWARD_TYPE.H))
                   {
                       if (myAwardPlayers[(int)AWARD_TYPE.H].award_val < h)
                       {
                           _logger.LogError("Award record error - H - my_val:{0}, target_val:{1}", myAwardPlayers[(int)AWARD_TYPE.H].award_val, h);
                           return true;
                       }
                   }
                   if (myAwardPlayers.ContainsKey((int)AWARD_TYPE.RBI))
                   {
                       if (myAwardPlayers[(int)AWARD_TYPE.RBI].award_val < rbi)
                       {
                           _logger.LogError("Award record error - RBI - my_val:{0}, target_val:{1}", myAwardPlayers[(int)AWARD_TYPE.RBI].award_val, rbi);
                           return true;
                       }
                   }
                   if (myAwardPlayers.ContainsKey((int)AWARD_TYPE.SB))
                   {
                       if (myAwardPlayers[(int)AWARD_TYPE.SB].award_val < sb)
                       {
                           _logger.LogError("Award record error - SB - my_val:{0}, target_val:{1}", myAwardPlayers[(int)AWARD_TYPE.SB].award_val, sb);
                           return true;
                       }
                   }
                   if (myAwardPlayers.ContainsKey((int)AWARD_TYPE.AVG))
                   {
                       if (myAwardPlayers[(int)AWARD_TYPE.AVG].award_val < avg)
                       {
                           _logger.LogError("Award record error - AVG - my_val:{0}, target_val:{1}", myAwardPlayers[(int)AWARD_TYPE.AVG].award_val, avg);
                           return true;
                       }
                   }

                   return false;
               }).Select(s => s.Key).ToList();

                if (resultPlayers.Count() > 0)
                {
                    return false;
                }

            }
            // 투수 일경우
            if (myAwardPlayers.ContainsKey((int)AWARD_TYPE.W) ||
                myAwardPlayers.ContainsKey((int)AWARD_TYPE.HLD) ||
                myAwardPlayers.ContainsKey((int)AWARD_TYPE.SV) ||
                myAwardPlayers.ContainsKey((int)AWARD_TYPE.SO) ||
                myAwardPlayers.ContainsKey((int)AWARD_TYPE.ERA))
            {
                List<string> resultPlayers = record.pitcher.Where(x =>
               {

                   string[] records = x.Value.Split(',');
                   int ip = int.Parse(records[(int)PITCHER_RECORD_TYPE.IP]);
                   int w = int.Parse(records[(int)PITCHER_RECORD_TYPE.W]);
                   int hld = int.Parse(records[(int)PITCHER_RECORD_TYPE.HLD]);
                   int sv = int.Parse(records[(int)PITCHER_RECORD_TYPE.SV]);
                   int so = int.Parse(records[(int)PITCHER_RECORD_TYPE.SO]);
                   int era = int.Parse(records[(int)PITCHER_RECORD_TYPE.ERA]);

                   if ((int)(ip / 3f) < GameCount)
                   {
                       return false;
                   }

                   if (myAwardPlayers.ContainsKey((int)AWARD_TYPE.W))
                   {
                       if (myAwardPlayers[(int)AWARD_TYPE.W].award_val < w)
                       {
                           _logger.LogError("Award record error - W - my_val:{0}, target_val:{1}", myAwardPlayers[(int)AWARD_TYPE.W].award_val, w);
                           return true;
                       }
                   }
                   if (myAwardPlayers.ContainsKey((int)AWARD_TYPE.HLD))
                   {
                       if (myAwardPlayers[(int)AWARD_TYPE.HLD].award_val < hld)
                       {
                           _logger.LogError("Award record error - HLD - my_val:{0}, target_val:{1}", myAwardPlayers[(int)AWARD_TYPE.HLD].award_val, hld);
                           return true;
                       }
                   }
                   if (myAwardPlayers.ContainsKey((int)AWARD_TYPE.SV))
                   {
                       if (myAwardPlayers[(int)AWARD_TYPE.SV].award_val < sv)
                       {
                           _logger.LogError("Award record error - SV - my_val:{0}, target_val:{1}", myAwardPlayers[(int)AWARD_TYPE.SV].award_val, sv);
                           return true;
                       }
                   }
                   if (myAwardPlayers.ContainsKey((int)AWARD_TYPE.SO))
                   {
                       if (myAwardPlayers[(int)AWARD_TYPE.SO].award_val < so)
                       {
                           _logger.LogError("Award record error - SO - my_val:{0}, target_val:{1}", myAwardPlayers[(int)AWARD_TYPE.SO].award_val, so);
                           return true;
                       }
                   }
                   if (myAwardPlayers.ContainsKey((int)AWARD_TYPE.ERA))
                   {
                       if (myAwardPlayers[(int)AWARD_TYPE.ERA].award_val > era)
                       {
                           _logger.LogError("Award record error - ERA - my_val:{0}, target_val:{1}", myAwardPlayers[(int)AWARD_TYPE.ERA].award_val, era);
                           return true;
                       }
                   }
                   return false;

               }).Select(s => s.Key).ToList();

                if (resultPlayers.Count() > 0)
                {
                    return false;
                }
            }

            return true;
        }
    }
}
