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

namespace ApiWebServer.Controllers.PlayerControllers
{
    [Route("api/Player/[controller]")]
    [ApiController]
    public class CoachPowerTrainingController : SessionContoller<ReqCoachPowerTraining, ResCoachPowerTraining>
    {
        public CoachPowerTrainingController(
            ILogger<CoachPowerTrainingController> logger,
            IConfiguration config, 
            IWebService<ReqCoachPowerTraining, ResCoachPowerTraining> webService, 
            IDBService dbService )
            : base( logger, config, webService, dbService )
        {
        }

        [HttpPost]
        [ApiExplorerSettings( GroupName = "client" )]
        [SwaggerExtend( "코치 지도력 연수", typeof(CoachPowerTrainingPacket) )]
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
            int powerTrainingCost = CacheManager.PBTable.ConstantTable.Const.coach_power_training_cost;

            DataSet dataSet = gameDB.USP_GS_GM_COACH_POWER_TRAINNING_R(webSession.TokenInfo.Pcid, reqData.AccountCoachIdx);
            if (dataSet == null)
            {
                return _webService.End(ErrorCode.ERROR_DB, "USP_GS_GM_COACH_POWER_TRAINNING_R");
            }

            DataSetWrapper dataSetWrapper = new DataSetWrapper(dataSet);
            AccountGame accountGameInfo = dataSetWrapper.GetObject<AccountGame>(0);
            long accountCoachIdx = dataSetWrapper.GetValue<long>(1, "account_coach_idx");
            AccountCoachPowerTrainingInfo accountCoachPowerTrainingInfo = dataSetWrapper.GetObject<AccountCoachPowerTrainingInfo>(2);
            AccountCoachLeadershipInfo accountCoachLeadershipInfo = dataSetWrapper.GetObject<AccountCoachLeadershipInfo>(3);

            //선수 있는지 체크
            if (accountCoachIdx == 0)
            {
                return _webService.End(ErrorCode.ERROR_NOT_COACH);
            }

            int maxGrade = CacheManager.PBTable.PlayerTable.GetMaxCoachPowerTrainingGrade();

            // 모두 a등급 달성했는지 체크
            if (accountCoachPowerTrainingInfo != null && accountCoachPowerTrainingInfo.coaching_psychology == maxGrade &&
                accountCoachPowerTrainingInfo.coaching_theory == maxGrade && accountCoachPowerTrainingInfo.communication == maxGrade &&
                accountCoachPowerTrainingInfo.technical_training_theory == maxGrade && accountCoachPowerTrainingInfo.training_theory == maxGrade)
            {
                return _webService.End(ErrorCode.ERROR_ALREADY_MAX_POWERTRAINING_GRADE);
            }
            // 지도력 연수 등급 뽑기
            CacheManager.PBTable.PlayerTable.CoachPowerTraning(accountCoachIdx, ref accountCoachPowerTrainingInfo);

            // 지도력 연수 등급에 따른 리더쉽 활성화 체크
            CheckCoachPowerTraningOpenLeadership(accountCoachPowerTrainingInfo, ref accountCoachLeadershipInfo, maxGrade);

            // 컨슘처리
            ConsumeReward consumeProcess = new ConsumeReward(webSession.TokenInfo.Pcid, gameDB, CONSUME_REWARD_TYPE.CONSUME, false);
            consumeProcess.AddConsume(new GameRewardInfo((byte)REWARD_TYPE.GOLD, 0, powerTrainingCost));
            ErrorCode consumeResult = consumeProcess.Run(ref accountGameInfo, true);

            if (consumeResult != ErrorCode.SUCCESS)
            {
                return _webService.End(consumeResult);
            }

            if (gameDB.USP_GS_GM_COACH_POWER_TRAINING(webSession.TokenInfo.Pcid, reqData.AccountCoachIdx, accountCoachPowerTrainingInfo, accountGameInfo, accountCoachLeadershipInfo) == false)
            {
                return _webService.End(ErrorCode.ERROR_DB, "USP_GS_GM_COACH_POWER_TRAINING");
            }

            resData.ResultAccountCurrency = accountGameInfo;
            resData.AccountCoachPowerTrainingInfo = accountCoachPowerTrainingInfo;
            resData.AccountCoachLeadershipInfo = accountCoachLeadershipInfo;

            return _webService.End();
        }
        private void CheckCoachPowerTraningOpenLeadership(AccountCoachPowerTrainingInfo accountCoachPowerTrainingInfo, ref AccountCoachLeadershipInfo accountCoachLeadershipInfo, int maxGrade)
        {
            int gradeCount = 0;

            // 리더십 개방 로직
            if (accountCoachPowerTrainingInfo.coaching_psychology == maxGrade)
                gradeCount += 1;
            if (accountCoachPowerTrainingInfo.coaching_theory == maxGrade)
                gradeCount += 1;
            if (accountCoachPowerTrainingInfo.communication == maxGrade)
                gradeCount += 1;
            if (accountCoachPowerTrainingInfo.technical_training_theory == maxGrade)
                gradeCount += 1;
            if (accountCoachPowerTrainingInfo.training_theory == maxGrade)
                gradeCount += 1;

            if (accountCoachLeadershipInfo == null)
            {
                accountCoachLeadershipInfo = new AccountCoachLeadershipInfo
                {
                    account_coach_idx = accountCoachPowerTrainingInfo.account_coach_idx,
                    leadership_idx1 = -1,
                    leadership_idx2 = -1,
                    leadership_idx3 = -1
                };
            }
            if (gradeCount >= PlayerDefine.Leadership1SlotNeedGrade && gradeCount < PlayerDefine.Leadership2SlotNeedGrade)
            {
                if (accountCoachLeadershipInfo.leadership_idx1 <= 0)
                    accountCoachLeadershipInfo.leadership_idx1 = 0;
            }
            else if (gradeCount >= PlayerDefine.Leadership2SlotNeedGrade && gradeCount < PlayerDefine.Leadership3SlotNeedGrade)
            {
                if (accountCoachLeadershipInfo.leadership_idx1 <= 0)
                    accountCoachLeadershipInfo.leadership_idx1 = 0;
                if (accountCoachLeadershipInfo.leadership_idx2 <= 0)
                    accountCoachLeadershipInfo.leadership_idx2 = 0;
            }
            else if (gradeCount == PlayerDefine.Leadership3SlotNeedGrade)
            {
                if (accountCoachLeadershipInfo.leadership_idx1 <= 0)
                    accountCoachLeadershipInfo.leadership_idx1 = 0;
                if (accountCoachLeadershipInfo.leadership_idx2 <= 0)
                    accountCoachLeadershipInfo.leadership_idx2 = 0;
                if (accountCoachLeadershipInfo.leadership_idx3 <= 0)
                    accountCoachLeadershipInfo.leadership_idx3 = 0;
            }
        }
    }
}
