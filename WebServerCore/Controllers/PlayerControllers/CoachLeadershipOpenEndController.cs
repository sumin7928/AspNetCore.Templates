using System.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using ApiWebServer.Core;
using ApiWebServer.Core.Controller;
using ApiWebServer.Core.Swagger;
using ApiWebServer.Database.Utils;
using ApiWebServer.Models;
using WebSharedLib.Contents;
using WebSharedLib.Contents.Api;
using WebSharedLib.Core.NPLib;
using WebSharedLib.Entity;
using WebSharedLib.Error;

namespace ApiWebServer.Controllers.PlayerControllers
{
    [Route("api/Player/[controller]")]
    [ApiController]
    public class CoachLeadershipOpenEndController : SessionContoller<ReqCoachLeadershipOpenEnd, ResCoachLeadershipOpenEnd>
    {
        public CoachLeadershipOpenEndController(
            ILogger<CoachLeadershipOpenEndController> logger,
            IConfiguration config, 
            IWebService<ReqCoachLeadershipOpenEnd, ResCoachLeadershipOpenEnd> webService, 
            IDBService dbService )
            : base( logger, config, webService, dbService )
        {
        }

        [HttpPost]
        [ApiExplorerSettings(GroupName = "client")]
        [SwaggerExtend("코치 능력(리더십) 오픈 / 재오픈 선택", typeof(CoachLeadershipOpenEndPacket))]
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


            // 포지션 변경하려는 슬롯이 열려 있는 슬롯인지, 껴져있는 선수가 있는지, 설정된 보직값과 비교하여 pb테이블 조건에 맞는지 체크.
            DataSet dataSet = gameDB.USP_GS_GM_COACH_LEADERSHIP_OPEN_END_R(webSession.TokenInfo.Pcid, reqData.AccountCoachIdx);
            if (dataSet == null)
            {
                return _webService.End(ErrorCode.ERROR_DB, "USP_GS_GM_COACH_LEADERSHIP_OPEN_END_R");
            }

            DataSetWrapper dataSetWrapper = new DataSetWrapper(dataSet);
            AccountCoach accountCoach = dataSetWrapper.GetObject<AccountCoach>(1);
            AccountCoachLeadershipInfo accountCoachLeadershipInfo = dataSetWrapper.GetObject<AccountCoachLeadershipInfo>(2);
            AccountTrainingResult trainingResult = dataSetWrapper.GetObject<AccountTrainingResult>(3);

            if (dataSetWrapper.GetRowCount(0) == 0)
            {
                return _webService.End(ErrorCode.ERROR_NO_ACCOUNT);
            }

            if (accountCoach == null)
            {
                return _webService.End(ErrorCode.ERROR_NOT_PLAYER);
            }

            if (accountCoachLeadershipInfo == null)
            {
                return _webService.End(ErrorCode.ERROR_INVALID_COACH_LEADERSHIP);
            }

            if (trainingResult == null)
            {
                return _webService.End(ErrorCode.ERROR_NOT_POTENTIAL_TRAINING_RESULT);
            }

            int changeLeadershipIdx = 0;

            if (reqData.IsChangeFlag == true)
            {
                if(reqData.SlotIdx == 1)
                    changeLeadershipIdx = trainingResult.select_idx1;
                else if (reqData.SlotIdx == 2)
                    changeLeadershipIdx = trainingResult.select_idx2;
                else if (reqData.SlotIdx == 3)
                    changeLeadershipIdx = trainingResult.select_idx3;
                else
                    return _webService.End(ErrorCode.ERROR_INVALID_PARAM);

                if(changeLeadershipIdx <= 0)
                    return _webService.End(ErrorCode.ERROR_INVALID_PARAM);
            }

            if (gameDB.USP_GS_GM_COACH_LEADERSHIP_OPEN_END(webSession.TokenInfo.Pcid, reqData.IsChangeFlag, reqData.AccountCoachIdx, reqData.SlotIdx, changeLeadershipIdx) == false)
            {
                return _webService.End(ErrorCode.ERROR_DB, "USP_GS_GM_COACH_LEADERSHIP_OPEN_END");
            }

            resData.CoachLeadershipIdx = changeLeadershipIdx;
            
            return _webService.End();
        }
    }
}
