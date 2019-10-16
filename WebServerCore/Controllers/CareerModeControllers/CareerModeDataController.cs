using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Data;
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

namespace ApiWebServer.Controllers.CareerModeControllers
{
    [Route("api/CareerMode/[controller]")]
    public class CareerModeDataController : SessionContoller<ReqCareerModeData, ResCareerModeData>
    {
        public CareerModeDataController(
            ILogger<CareerModeDataController> logger,
            IConfiguration config,
            IWebService<ReqCareerModeData, ResCareerModeData> webService,
            IDBService dbService )
            : base( logger, config, webService, dbService )
        {
        }

        [HttpPost]
        [ApiExplorerSettings( GroupName = "client" )]
        [SwaggerExtend( "커리어모드 데이터(로그인시 1회만)", typeof(CareerModeDataPacket) )]
        public NPWebResponse Controller([FromBody] NPWebRequest requestBody )
        {
            WrapWebService( requestBody );
            if ( _webService.ErrorCode != ErrorCode.SUCCESS )
            {
                return _webService.End();
            }

            // Business
            var webSession = _webService.WebSession;
            var reqData = _webService.WebPacket.ReqData;
            var resData = _webService.WebPacket.ResData;
            var gameDB = _dbService.CreateGameDB( _webService.RequestNo, webSession.DBNo );

            DataSet dataSet = gameDB.USP_GS_GM_CAREERMODE_DATA_R(webSession.TokenInfo.Pcid);
            if (dataSet == null)
            {
                return _webService.End(ErrorCode.ERROR_DB, "USP_GS_GM_CAREERMODE_DATA_R");
            }

            DataSetWrapper dataSetWrapper = new DataSetWrapper(dataSet);
            CareerModeInfo careerModeInfo = dataSetWrapper.GetObject<CareerModeInfo>(0);

            if (careerModeInfo == null || careerModeInfo.team_idx == 0)
            {
                resData.Player = null;
                resData.SpringCampInfo = null;
            }
            else
            {
                resData.Player = dataSetWrapper.GetObjectList<CareerModePlayer>(1);
                resData.SpringCampInfo = dataSetWrapper.GetObjectList<CareerModeSpringCamp>(2);
                resData.SpecialTrainingInfo = dataSetWrapper.GetObjectList<CareerModeSpecialTraining>(3);
                resData.MissionList = dataSetWrapper.GetObjectList<CareerModeMission>(4);
                resData.EventList = dataSetWrapper.GetObjectList<CareerModeCycleEventInfo>(5);
            }

            return _webService.End();
        }
    }
}
