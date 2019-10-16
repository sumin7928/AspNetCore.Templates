using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Data;
using ApiWebServer.Cache;
using ApiWebServer.Core;
using ApiWebServer.Core.Controller;
using ApiWebServer.Core.Swagger;
using ApiWebServer.Database.Utils;
using ApiWebServer.Logic;
using ApiWebServer.Models;
using WebSharedLib.Contents;
using WebSharedLib.Contents.Api;
using WebSharedLib.Core.NPLib;
using WebSharedLib.Error;
using WebSharedLib.Entity;
using ApiWebServer.Common.Define;
using System.Collections.Generic;

namespace ApiWebServer.Controllers.PlayerControllers
{
    [Route("api/Player/[controller]")]
    [ApiController]
    public class CoachPassOnLeadershipStartController : SessionContoller<ReqCoachPassOnLeadershipStart, ResCoachPassOnLeadershipStart>
    {
        public CoachPassOnLeadershipStartController(
            ILogger<CoachPassOnLeadershipStartController> logger,
            IConfiguration config, 
            IWebService<ReqCoachPassOnLeadershipStart, ResCoachPassOnLeadershipStart> webService, 
            IDBService dbService )
            : base( logger, config, webService, dbService )
        {
        }

        [HttpPost]
        [ApiExplorerSettings( GroupName = "client" )]
        [SwaggerExtend( "코치 리더쉽 전수", typeof(CoachPassOnLeadershipStartPacket) )]
        public NPWebResponse Controller( [FromBody] NPWebRequest requestBody )
        {
            WrapWebService( requestBody );
            if ( _webService.ErrorCode != ErrorCode.SUCCESS)
            {
                return _webService.End( _webService.ErrorCode );
            }

            // Business
            var webSession = _webService.WebSession;
            var reqData = _webService.WebPacket.ReqData;
            var resData = _webService.WebPacket.ResData;
            var gameDB = _dbService.CreateGameDB(_webService.RequestNo, webSession.DBNo);

            if (reqData.MaterialCoachIdx.Count != PlayerDefine.CoachLeadershipMaterialCount)
            {
                return _webService.End(ErrorCode.ERROR_NOT_MATCHING_MATERIAL_COUNT);
            }

            DataSet dataSet = gameDB.USP_GS_GM_COACH_PASSON_LEADERSHIP_START_R(webSession.TokenInfo.Pcid, reqData.AccountCoachIdx, reqData.MaterialCoachIdx);
            if (dataSet == null)
            {
                return _webService.End(ErrorCode.ERROR_DB, "USP_GS_GM_COACH_PASSON_LEADERSHIP_START_R");
            }

            DataSetWrapper dataSetWrapper = new DataSetWrapper(dataSet);
            AccountGame accountGameInfo = dataSetWrapper.GetObject<AccountGame>(0);
            Coach accountCoachInfo = dataSetWrapper.GetObject<Coach>(1);
            List<Coach> materialCoachListInfo = dataSetWrapper.GetObjectList<Coach>(2);
            bool isAlreadyFlag = dataSetWrapper.GetValue<bool>(3, "use_flag");

            //유저 데이터 확인
            if (accountGameInfo == null)
            {
                return _webService.End(ErrorCode.ERROR_ACCOUNT);
            }
            if (accountCoachInfo == null)
            {
                return _webService.End(ErrorCode.ERROR_NOT_COACH);
            }

            //이미 진행하고 있는 훈련이 있다면 에러
            //임시처리 ( 나중에 꼭 주석풀어야함)
            /*if (isAlreadyFlag == true)
            {
                return _webService.End(ErrorCode.ERROR_REQUEST_DATA);
            }*/

            if (materialCoachListInfo.FindAll(x => x.account_coach_idx == accountCoachInfo.account_coach_idx).Count > 0)
            {
                return _webService.End(ErrorCode.ERROR_INVALID_COACH_DATA);
            }
            if (reqData.MaterialCoachIdx.Count != materialCoachListInfo.Count)
            {
                return _webService.End(ErrorCode.ERROR_INVALID_COACH_MATERIAL);
            }
            if (accountCoachInfo.leadership_idx1 <= 0 && accountCoachInfo.leadership_idx2 <= 0 && accountCoachInfo.leadership_idx3 <= 0)
            {
                return _webService.End(ErrorCode.ERROR_NOT_OPEN_COACH_LEADERSHIP);
            }

            //보정치 계산
            int correction = GetRankupCorrection(materialCoachListInfo);

            // 리더쉽 강화
            CacheManager.PBTable.PlayerTable.LeadershipRankup(accountCoachInfo, correction, out AccountTrainingResult accountTrainingResult);

            GameRewardInfo rewardInfo = new GameRewardInfo((byte)CacheManager.PBTable.ConstantTable.Const.leadership_rankup_cost_type, 0, CacheManager.PBTable.ConstantTable.Const.leadership_rankup_cost_count);
            ConsumeReward consumeProcess = new ConsumeReward(webSession.TokenInfo.Pcid, gameDB, CONSUME_REWARD_TYPE.CONSUME, false);
            consumeProcess.AddConsume(rewardInfo);
            ErrorCode consumeResult = consumeProcess.Run(ref accountGameInfo, true);

            if (consumeResult != ErrorCode.SUCCESS)
            {
                return _webService.End(consumeResult);
            }

            accountGameInfo.now_coach -= materialCoachListInfo.Count;

            if (gameDB.USP_GS_GM_COACH_PASSON_LEADERSHIP_START(webSession.TokenInfo.Pcid, reqData.AccountCoachIdx, accountTrainingResult, accountGameInfo, reqData.MaterialCoachIdx) == false)
            {
                return _webService.End(ErrorCode.ERROR_DB, "USP_GS_GM_COACH_PASSON_LEADERSHIP_START");
            }

            resData.CoachLeadershipIdx1 = accountTrainingResult.select_idx1;
            resData.CoachLeadershipIdx2 = accountTrainingResult.select_idx2;
            resData.CoachLeadershipIdx3 = accountTrainingResult.select_idx3;
            resData.NowHaveCoachCount = accountGameInfo.now_coach;
            resData.ResultAccountCurrency = accountGameInfo;

            return _webService.End();
        }

        private int GetRankupCorrection(List<Coach> materialCoachListInfo)
        {
            int correction = 0;

            foreach (var data in materialCoachListInfo)
            {
                correction += CacheManager.PBTable.PlayerTable.GetCoachData(data.coach_idx).power;
                correction += CacheManager.PBTable.PlayerTable.GetCoachAddedReinforcePower(data.coaching_psychology,
                                                                                                    data.coaching_theory,
                                                                                                    data.technical_training_theory,
                                                                                                    data.training_theory,
                                                                                                    data.communication);
                correction /= CacheManager.PBTable.ConstantTable.Const.leadership_rankup_power_const;

                if (data.leadership_idx1 > 0)
                    correction += CacheManager.PBTable.ConstantTable.Const.leadership_rankup_skill_const;
                if (data.leadership_idx2 > 0)
                    correction += CacheManager.PBTable.ConstantTable.Const.leadership_rankup_skill_const;
                if (data.leadership_idx3 > 0)
                    correction += CacheManager.PBTable.ConstantTable.Const.leadership_rankup_skill_const;
            }

            return correction;
        }
    }
}
