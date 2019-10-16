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
    public class CoachPassOnCoachingSkillController : SessionContoller<ReqCoachPassOnCoachingSkill, ResCoachPassOnCoachingSkill>
    {
        public CoachPassOnCoachingSkillController(
            ILogger<CoachPassOnCoachingSkillController> logger,
            IConfiguration config, 
            IWebService<ReqCoachPassOnCoachingSkill, ResCoachPassOnCoachingSkill> webService, 
            IDBService dbService )
            : base( logger, config, webService, dbService )
        {
        }

        [HttpPost]
        [ApiExplorerSettings( GroupName = "client" )]
        [SwaggerExtend( "코치 코칭스킬 전수", typeof(CoachPassOnCoachingSkillPacket) )]
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

            int knowhowCostType = 0;
            int knowhowCost = 0;
            // TODO: 보정치 계산 상수 추후 const로 배자.

            DataSet dataSet = gameDB.USP_GS_GM_COACH_PASSON_COACHINGSKILL_R(webSession.TokenInfo.Pcid, reqData.AccountCoachIdx, reqData.MaterialCoachIdx);
            if (dataSet == null)
            {
                return _webService.End(ErrorCode.ERROR_DB, "USP_GS_GM_COACH_PASSON_COACHINGSKILL_R");
            }

            DataSetWrapper dataSetWrapper = new DataSetWrapper(dataSet);
            AccountGame accountGameInfo = dataSetWrapper.GetObject<AccountGame>(0);
            Coach accountCoachInfo = dataSetWrapper.GetObject<Coach>(1);
            Coach materialCoachListInfo = dataSetWrapper.GetObject<Coach>(2);
            AccountTrainingResult accountTrainingResult = new AccountTrainingResult();

            (byte costType, int costValue) = CacheManager.PBTable.PlayerTable.GetCoachingSkillRankUpCost(accountCoachInfo.coaching_skill);
            knowhowCostType = costType;
            knowhowCost = costValue;

            //유저 데이터 확인
            if (accountGameInfo == null)
            {
                return _webService.End(ErrorCode.ERROR_ACCOUNT);
            }
            if (accountCoachInfo == null)
            {
                return _webService.End(ErrorCode.ERROR_NOT_COACH);
            }
            if (materialCoachListInfo == null)
            {
                return _webService.End(ErrorCode.ERROR_INVALID_COACH_MATERIAL);
            }
            if (accountCoachInfo.coach_idx != materialCoachListInfo.coach_idx)
            {
                return _webService.End(ErrorCode.ERROR_NOT_MATCHING_COACH_WITH_MATERIAL);
            }

            ErrorCode passOnKnowHowResult = 0;

            passOnKnowHowResult = CacheManager.PBTable.PlayerTable.CoachingSkillRankUp(accountCoachInfo.coaching_skill, ref accountCoachInfo.coaching_skill_failrevision, out int nextCoachingSkillIdx);

            if (passOnKnowHowResult != ErrorCode.SUCCESS)
            {
                return _webService.End(passOnKnowHowResult);
            }

            ConsumeReward consumeProcess = new ConsumeReward(webSession.TokenInfo.Pcid, gameDB, Common.Define.CONSUME_REWARD_TYPE.CONSUME, false);
            consumeProcess.AddConsume(new GameRewardInfo((byte)REWARD_TYPE.GOLD, 0, knowhowCost));
            ErrorCode consumeResult = consumeProcess.Run(ref accountGameInfo, true);

            if (consumeResult != ErrorCode.SUCCESS)
            {
                return _webService.End(consumeResult);
            }

            if (materialCoachListInfo != null)
            {
                accountGameInfo.now_coach -= 1;
            }            

            if (gameDB.USP_GS_GM_COACH_PASSON_COACHINGSKILL(webSession.TokenInfo.Pcid, reqData.AccountCoachIdx, nextCoachingSkillIdx, accountGameInfo, accountCoachInfo.coaching_skill_failrevision, reqData.MaterialCoachIdx) == false)
            {
                return _webService.End(ErrorCode.ERROR_DB, "USP_GS_GM_COACH_PASSON_COACHINGSKILL");
            }

            resData.CoachingSkillIdx = nextCoachingSkillIdx;
            resData.NowHaveCoachCount = accountGameInfo.now_coach;
            resData.ResultAccountCurrency = accountGameInfo;
            resData.ReinforceAddRate = accountCoachInfo.coaching_skill_failrevision;

            return _webService.End();
        }
    }
}
