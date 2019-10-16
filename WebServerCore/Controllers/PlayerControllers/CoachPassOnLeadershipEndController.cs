using System.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using ApiWebServer.Core;
using ApiWebServer.Core.Controller;
using ApiWebServer.Core.Swagger;
using ApiWebServer.Database.Utils;
using WebSharedLib.Contents;
using WebSharedLib.Contents.Api;
using WebSharedLib.Core.NPLib;
using WebSharedLib.Entity;
using WebSharedLib.Error;

namespace ApiWebServer.Controllers.PlayerControllers
{
    [Route("api/Player/[controller]")]
    [ApiController]
    public class CoachPassOnLeadershipEndController : SessionContoller<ReqCoachPassOnLeadershipEnd, ResCoachPassOnLeadershipEnd>
    {
        public CoachPassOnLeadershipEndController(
            ILogger<CoachPassOnLeadershipEndController> logger,
            IConfiguration config, 
            IWebService<ReqCoachPassOnLeadershipEnd, ResCoachPassOnLeadershipEnd> webService, 
            IDBService dbService )
            : base( logger, config, webService, dbService )
        {
        }

        [HttpPost]
        [ApiExplorerSettings( GroupName = "client" )]
        [SwaggerExtend( "코치 리더쉽 전수 확정", typeof(CoachPassOnLeadershipEndPacket) )]
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

            DataSet dataSet = gameDB.USP_GS_GM_COACH_PASSON_LEADERSHIP_END_R(webSession.TokenInfo.Pcid, reqData.AccountCoachIdx);
            if (dataSet == null)
            {
                return _webService.End(ErrorCode.ERROR_DB, "USP_GS_GM_COACH_PASSON_LEADERSHIP_END_R");
            }

            DataSetWrapper dataSetWrapper = new DataSetWrapper(dataSet);
            AccountTrainingResult accountLeadershipTrainingInfo = dataSetWrapper.GetObject<AccountTrainingResult>(0);

            //유저 데이터 확인
            if (accountLeadershipTrainingInfo == null || (accountLeadershipTrainingInfo.select_idx1 <= 0 && accountLeadershipTrainingInfo.select_idx2 <= 0 && accountLeadershipTrainingInfo.select_idx3 <= 0))
            {
                return _webService.End(ErrorCode.ERROR_NOT_COACH);
            }

            if (gameDB.USP_GS_GM_COACH_PASSON_LEADERSHIP_END(webSession.TokenInfo.Pcid, reqData.AccountCoachIdx, accountLeadershipTrainingInfo, reqData.SelectFlag) == false)
            {
                return _webService.End(ErrorCode.ERROR_DB, "USP_GS_GM_COACH_PASSON_LEADERSHIP_END");
            }
            if (reqData.SelectFlag == true)
            {
                resData.AccountTrainingInfo = accountLeadershipTrainingInfo;
            }

            return _webService.End();
        }
    }
}
