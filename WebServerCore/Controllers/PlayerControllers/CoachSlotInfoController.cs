using System.Collections.Generic;
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
    public class CoachSlotInfoController : SessionContoller<ReqCoachSlotInfo, ResCoachSlotInfo>
    {
        public CoachSlotInfoController(
            ILogger<CoachSlotInfoController> logger,
            IConfiguration config, 
            IWebService<ReqCoachSlotInfo, ResCoachSlotInfo> webService, 
            IDBService dbService )
            : base( logger, config, webService, dbService )
        {
        }

        [HttpPost]
        [ApiExplorerSettings( GroupName = "client" )]
        [SwaggerExtend( "코치 슬롯 정보", typeof( CoachSlotInfoPacket ) )] 
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

            DataSet dataSet = gameDB.USP_GS_GM_COACH_SLOT_INFO_R(webSession.TokenInfo.Pcid, reqData.ModeType);
            if (dataSet == null)
            {
                return _webService.End(ErrorCode.ERROR_DB, "USP_GS_GM_COACH_SLOT_INFO_R");
            }

            DataSetWrapper dataSetWrapper = new DataSetWrapper(dataSet);
            List<CoachSlot> listCoachSlotInfo = dataSetWrapper.GetObjectList<CoachSlot>(0);
            resData.CoachSlotInfo = listCoachSlotInfo;

            return _webService.End();
        }
    }
}
