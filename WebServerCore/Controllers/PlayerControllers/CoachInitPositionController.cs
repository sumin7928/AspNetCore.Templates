using System.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using ApiWebServer.Cache;
using ApiWebServer.Core;
using ApiWebServer.Core.Controller;
using ApiWebServer.Core.Swagger;
using ApiWebServer.Database.Utils;
using ApiWebServer.Models;
using WebSharedLib.Contents;
using WebSharedLib.Contents.Api;
using WebSharedLib.Core.NPLib;
using WebSharedLib.Error;

namespace ApiWebServer.Controllers.PlayerControllers
{
    [Route("api/Player/[controller]")]
    [ApiController]
    public class CoachInitPositionController : SessionContoller<ReqCoachInitPosition, ResCoachInitPosition>
    {
        public CoachInitPositionController(
            ILogger<CoachInitPositionController> logger,
            IConfiguration config, 
            IWebService<ReqCoachInitPosition, ResCoachInitPosition> webService, 
            IDBService dbService )
            : base( logger, config, webService, dbService )
        {
        }

        [HttpPost]
        [ApiExplorerSettings( GroupName = "client" )]
        [SwaggerExtend( "코치 보직 설정 변경", typeof( CoachInitPositionPacket ) )] 
        public NPWebResponse Controller([FromBody] NPWebRequest requestBody )
        {
            WrapWebService( requestBody );
            if ( _webService.ErrorCode != ErrorCode.SUCCESS )
            {
                return _webService.End( _webService.ErrorCode );
            }

            // Business
            var webSession = _webService.WebSession;
            var reqData = _webService.WebPacket.ReqData;
            var resData = _webService.WebPacket.ResData;
            var gameDB = _dbService.CreateGameDB( _webService.RequestNo, webSession.DBNo );

            // 포지션 변경하려는 슬롯이 열려 있는 슬롯인지, 껴져있는 선수가 있는지, 설정된 보직값과 비교하여 pb테이블 조건에 맞는지 체크.
            DataSet dataSet = gameDB.USP_GS_GM_COACH_INIT_POSITION_R(webSession.TokenInfo.Pcid, reqData.SlotIdx, reqData.Position);
            if (dataSet == null)
            {
                return _webService.End(ErrorCode.ERROR_DB, "USP_GS_GM_COACH_INIT_POSITION_R" );
            }

            DataSetWrapper dataSetWrapper = new DataSetWrapper(dataSet);

            AccountCoach accountCoach = dataSetWrapper.GetObject<AccountCoach>(0);  
            int coachPositionCnt = dataSetWrapper.GetValue<int>(1, "position_cnt");
           
            ErrorCode initPositionResult = CacheManager.PBTable.PlayerTable.InitCoachPosition(reqData, accountCoach, coachPositionCnt, out byte deleteFlag, out long rtnAccountCoachIdx);
            if (initPositionResult != ErrorCode.SUCCESS)
            {
                return _webService.End(initPositionResult);
            }
            
            if (gameDB.USP_GS_GM_COACH_INIT_POSITION(webSession.TokenInfo.Pcid, reqData.SlotIdx, reqData.Position, deleteFlag) == false)
            {
                return _webService.End(ErrorCode.ERROR_DB, "USP_GS_GM_COACH_INIT_POSITION" );
            }

            resData.AccountCoachIdx = rtnAccountCoachIdx;

            return _webService.End();
        }
    }
}
