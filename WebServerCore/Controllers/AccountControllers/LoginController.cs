using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using ApiWebServer.Cache;
using ApiWebServer.Common.Define;
using ApiWebServer.Core;
using ApiWebServer.Core.Controller;
using ApiWebServer.Core.Helper;
using ApiWebServer.Core.Swagger;
using ApiWebServer.Database.Utils;
using ApiWebServer.Logic;
using ApiWebServer.Models;
using ApiWebServer.PBTables;
using WebSharedLib.Contents;
using WebSharedLib.Contents.Api;
using WebSharedLib.Core.NPLib;
using WebSharedLib.Entity;
using WebSharedLib.Error;

namespace ApiWebServer.Controllers.AccountControllers
{
    [Route("api/Account/[controller]")]
    [ApiController]
    public class LoginController : NonSessionController<ReqLogin, ResLogin>
    {
        public LoginController(
            ILogger<LoginController> logger,
            IConfiguration config,
            IWebService<ReqLogin, ResLogin> webService,
            IDBService dbService)
            : base(logger, config, webService, dbService)
        {
        }

        [HttpPost]
        [ApiExplorerSettings(GroupName = "client")]
        [SwaggerExtend("로그인", typeof(LoginPacket))]
        public NPWebResponse Controller([FromBody] NPWebRequest requestBody)
        {
            WrapWebService(requestBody);
            if (_webService.ErrorCode != ErrorCode.SUCCESS)
            {
                return _webService.End(_webService.ErrorCode);
            }

            // Business
            var reqData = _webService.WebPacket.ReqData;
            var resData = _webService.WebPacket.ResData;

            if (string.IsNullOrEmpty(reqData.PubID) == true)
            {
                return _webService.End(ErrorCode.ERROR_INVALID_PARAM);
            }

            // AccoutDB 계정 정보 가져옴
            var accoutDB = _dbService.CreateAccountDB(_webService.RequestNo);
            DataSet accountDataSet = accoutDB.USP_AC_ACCOUNT_INFO_R(reqData.PubType, reqData.PubID);
            if (accountDataSet == null)
            {
                return _webService.End(ErrorCode.ERROR_DB, "USP_AC_ACCOUNT_INFO_R");
            }

            DataSetWrapper accountDataSetWrapper = new DataSetWrapper(accountDataSet);
            Account accountInfo = accountDataSetWrapper.GetObject<Account>(0);

            if (accountInfo == null)
            {
                return _webService.End(ErrorCode.ERROR_NO_ACCOUNT);
            }

            if (accountInfo.is_out_user == 1)
            {
                return _webService.End(ErrorCode.ERROR_SECESSION_ACCOUNT);
            }

            if (accountInfo.block_range > 0)
            {
                _webService.WebPacket.ResHeader.ShowMessage = Encoding.UTF8.GetBytes(accountInfo.block_reason);
                return _webService.End(ErrorCode.ERROR_BLOCK_ACCOUNT);
            }

            // GameDB 계정 정보 가져옴.
            var gameDB = _dbService.CreateGameDB(_webService.RequestNo, accountInfo.db_num);
            DataSet gameDataSet = gameDB.USP_GS_GM_ACCOUNT_LOGIN_R(accountInfo.pc_id);
            if (gameDataSet == null)
            {
                return _webService.End(ErrorCode.ERROR_DB, "USP_GS_GM_ACCOUNT_LOGIN_R");
            }

            DataSetWrapper gameDataSetWrapper = new DataSetWrapper(gameDataSet);
            AccountGame accountGameInfo = gameDataSetWrapper.GetObject<AccountGame>(0);
            //List<RepeatMission> missionList = gameDataSetWrapper.GetObjectList<RepeatMission>(1);
            //List<Achievement> achivementList = gameDataSetWrapper.GetObjectList<Achievement>(2);
            List<SkillMastery> skillMasteryInfo = gameDataSetWrapper.GetObjectList<SkillMastery>(1);
            string masteryAttackCondition = gameDataSetWrapper.GetValue<string>(2, "attack_condition");
            string masteryDefenseCondition = gameDataSetWrapper.GetValue<string>(2, "defense_condition");

            // 스킬 마스터리 컨디션 저장
            SkillMasteryCondition skillMasteryCondition = new SkillMasteryCondition
            {
                attack_condition = JsonConvert.DeserializeObject<List<int>>(masteryAttackCondition),
                defense_condition = JsonConvert.DeserializeObject<List<int>>(masteryDefenseCondition)
            };

            // 미션 & 업적 갱신 체크
            //List<RepeatMission> newMissionList = null;
            //List<Achievement> newAchivementList = CacheManager.PBTable.MissionAchievementTable.GetFirstAchievement(achivementList);

            // 최초 생성 시 
            /*if (missionList != null && missionList.Count == 0)
            {
                newMissionList = CacheManager.PBTable.MissionAchievementTable.GetDailyMission(accountInfo.date_time);
            }
            // 일일 로그인 체크
            else if (accountInfo.is_firsttime_flag == 1)
            {
                // 미션 정보 바꿔줌
                newMissionList = CacheManager.PBTable.MissionAchievementTable.GetDailyMission(accountInfo.date_time);
                missionList = newMissionList;
            }

            // 미션 & 업적 처리 생성
            MissionAchievement missionAchievement = new MissionAchievement(accountInfo.pc_id, gameDB, _webService.WebPacket.ResHeader);
            missionAchievement.Input(missionList, achivementList);*/

            // 액션 타입 넣어줌
            // 로그인쪽 미션 및 업적은 없어서 현재 처리 프로세스 주석 
            //missionAchievement.AddAction(1, 1);

            // 로그인 시간 저장
            if (accoutDB.USP_GS_AC_ACCOUNT_DETAIL_LOGIN_TIME_U(accountInfo.pc_id, accountInfo.is_firsttime_flag == 1 ? reqData.StoreType : -1) == false)
            {
                return _webService.End(ErrorCode.ERROR_DB, "USP_GS_AC_ACCOUNT_DETAIL_LOGIN_TIME_U");
            }

            // 게임 정보 저장
            /*string newMissions = newMissionList != null ? JsonConvert.SerializeObject(newMissionList) : "";
            string newAchievements = newAchivementList != null ? JsonConvert.SerializeObject(newAchivementList) : "";
            if (gameDB.USP_GS_GM_ACCOUNT_LOGIN(accountInfo.pc_id, newMissions, newAchievements) == false)
            {
                return _webService.End(ErrorCode.ERROR_DB, "USP_GS_GM_ACCOUNT_LOGIN");
            }

            // 미션 업적 정보 저장
            if (missionAchievement.Update() == false)
            {
                return _webService.End(ErrorCode.ERROR_FAILED_UPDATE_DATA);
            }*/

            // 로그인 세션 저장
            WebSession webSession = CreateWebSession(reqData, accountInfo, accountGameInfo);
            if (_webService.WrapLoginSession(webSession) == false)
            {
                return _webService.End(_webService.ErrorCode);
            }

            resData.UTeamIax = webSession.Token;
            resData.TeamIax = Encoding.UTF8.GetString(AppConfig.ClientCryptKey);
            resData.FlowIax = Encoding.UTF8.GetString(AppConfig.ClientCryptIV);
            resData.PC_ID = accountInfo.pc_id;
            resData.EncryptProfileUrl = webSession.EncryptedProfileUrl;
            resData.NickName = accountInfo.pc_name;
            resData.TeamIdx = accountGameInfo.team_idx;

            resData.ManagerLv = accountGameInfo.user_lv;
            resData.ManagerExp = accountGameInfo.user_exp;
            resData.AccountCurrency = accountGameInfo;
            resData.CoachSlotIdx = accountGameInfo.coach_slot_idx;

            resData.MasteryCondition = skillMasteryCondition;
            resData.MasteryIdxList = skillMasteryInfo;
            //resData.MissionList = missionAchievement.MissionList;
            //resData.AchievementList = missionAchievement.AchievementList;

            return _webService.End();
        }

        private WebSession CreateWebSession(ReqLogin reqData, Account accountInfo, AccountGame accountGame)
        {
            string profileUrl = string.Empty;
            if (reqData.ProfileUrl != null && reqData.ProfileUrl.Length > 0)
            {
                profileUrl = NPCrypt.Encrypt(reqData.ProfileUrl, AppConfig.ProfileURLKey, AppConfig.ProfileURLIV);
            }

            return new WebSession
            {
                TokenInfo = new WebSession.WebTokenInfo
                {
                    Pcid = accountInfo.pc_id,
                    SessionKey = WebSessionHelper.CreateSessionKey(),
                    ConnTime = DateTime.Now.ToString()
                },

                PubId = reqData.PubID,
                PubType = reqData.PubType,
                EncryptedProfileUrl = profileUrl,
                OSType = reqData.OSType,
                StoreType = reqData.StoreType,
                Version = reqData.Version,
                TeamIdx = accountGame.team_idx,
                NationType = accountGame.nation_type,
                UserName = accountInfo.pc_name,
                DBNo = accountInfo.db_num,
                Sequence = 2,
                MissionList = null,
                AchievementList = null
            };
        }
    }
}
