using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Data;
using ApiWebServer.Cache;
using ApiWebServer.Core;
using ApiWebServer.Core.Swagger;
using WebSharedLib.Contents;
using WebSharedLib.Core.NPLib;
using WebSharedLib.Error;
using WebSharedLib.Entity;
using ApiWebServer.Common.Define;
using ApiWebServer.Database.Utils;
using ApiWebServer.PBTables;
using WebSharedLib.Contents.Api;
using ApiWebServer.Models;
using ApiWebServer.Core.Controller;
using System.Text;

namespace ApiWebServer.Controllers.CareerModeControllers
{
    [Route("api/CareerMode/[controller]")]
    [ApiController]
    public class CareerModeCycleEventSelectController : SessionContoller<ReqCareerModeCycleEventSelect, ResCareerModeCycleEventSelect>
    {
        public CareerModeCycleEventSelectController(
            ILogger<CareerModeCycleEventSelectController> logger,
            IConfiguration config,
            IWebService<ReqCareerModeCycleEventSelect, ResCareerModeCycleEventSelect> webService,
            IDBService dbService)
            : base(logger, config, webService, dbService)
        {
        }

        [HttpPost]
        [ApiExplorerSettings(GroupName = "client")]
        [SwaggerExtend("커리어모드 관리주기 이벤트 선택", typeof(CareerModeCycleEventSelectPacket))]
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

            DataSet dataSet = gameDB.USP_GS_GM_CAREERMODE_CYCLE_EVENT_SELECT_R(webSession.TokenInfo.Pcid, reqData.event_idx);
            if (dataSet == null)
            {
                return _webService.End(ErrorCode.ERROR_DB, "USP_GS_GM_CAREERMODE_CYCLE_EVENT_SELECT_R");
            }

            DataSetWrapper dataSetWrapper = new DataSetWrapper(dataSet);
            byte career_no = dataSetWrapper.GetValue<byte>(0, "career_no");
            CareerModeCycleEventInfo cycleEventInfo = dataSetWrapper.GetObject<CareerModeCycleEventInfo>(1);

            // 유효성 체크
            if (reqData.CareerNo != career_no)
            {
                return _webService.End(ErrorCode.ERROR_NOT_MATCHING_INFO);
            }
            else if (cycleEventInfo == null)
            {
                return _webService.End(ErrorCode.ERROR_NOT_FOUND_EVENT_INFO);
            }
            else if (cycleEventInfo.select_idx != 0)
            {
                return _webService.End(ErrorCode.ERROR_ALREADY_EVENT_SELECT_INFO);
            }

            if (false == CacheManager.PBTable.CareerModeTable.IsValidCycleEventSelectIdx(reqData.event_idx, reqData.select_idx))
            {
                return _webService.End(ErrorCode.ERROR_INVALID_EVENT_SELECT_INFO);
            }

            if (gameDB.USP_GS_GM_CAREERMODE_CYCLE_EVENT_SELECT(webSession.TokenInfo.Pcid, reqData.event_idx, reqData.select_idx) == false)
            {
                return _webService.End(ErrorCode.ERROR_DB, "USP_GS_GM_CAREERMODE_CYCLE_EVENT_SELECT");
            }

            return _webService.End();
        }
    }
}
