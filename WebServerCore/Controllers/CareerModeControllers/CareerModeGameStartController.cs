using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MsgPack.Serialization;
using System.Collections.Generic;
using System.Data;
using ApiWebServer.Cache;
using ApiWebServer.Common.Define;
using ApiWebServer.Core;
using ApiWebServer.Core.Controller;
using ApiWebServer.Core.Swagger;
using ApiWebServer.Database.Utils;
using ApiWebServer.Logic;
using ApiWebServer.Models;
using WebSharedLib.Contents;
using WebSharedLib.Contents.Api;
using WebSharedLib.Core.NPLib;
using WebSharedLib.Entity;
using WebSharedLib.Error;
using System.Text;
using ApiWebServer.Common;

namespace ApiWebServer.Controllers.CareerModeControllers
{
    [Route("api/CareerMode/[controller]")]
    [ApiController]
    public class CareerModeGameStartController : SessionContoller<ReqCareerModeGameStart, ResCareerModeGameStart>
    {
        public CareerModeGameStartController(
            ILogger<CareerModeGameStartController> logger,
            IConfiguration config,
            IWebService<ReqCareerModeGameStart, ResCareerModeGameStart> webService,
            IDBService dbService)
            : base(logger, config, webService, dbService)
        {
        }

        [HttpPost]
        [ApiExplorerSettings(GroupName = "client")]
        [SwaggerExtend("커리어모드 게임 시작", typeof(CareerModeGameStartPacket))]
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

            int checkItemIdx = 0;
            byte simulItemUse = 0;

            if (reqData.isSimulItemUse == true)
            {
                checkItemIdx = CacheManager.PBTable.ItemTable.itemIdxCareermodeSimulration;
                simulItemUse = 1;
            }

            DataSet dataSet = gameDB.USP_GS_GM_CAREERMODE_GAME_START_R(webSession.TokenInfo.Pcid, checkItemIdx);
            if (dataSet == null)
            {
                return _webService.End(ErrorCode.ERROR_DB, "USP_GS_GM_CAREERMODE_GAME_START_R");
            }

            DataSetWrapper dataSetWrapper = new DataSetWrapper(dataSet);
            CareerModeInfo careerModeInfo = dataSetWrapper.GetObject<CareerModeInfo>(0);

            if(careerModeInfo == null)
            {
                return _webService.End(ErrorCode.ERROR_DB_DATA);
            }

            // 현재 커리어 모드 검증 
            if (reqData.CareerNo != careerModeInfo.career_no ||
                reqData.DegreeNo != careerModeInfo.degree_no ||
                reqData.MatchGroup != careerModeInfo.match_group)
            {
                return _webService.End(ErrorCode.ERROR_NOT_MATCHING_INFO);
            }
            else if (careerModeInfo.springcamp_step != (byte)SPRING_CAMP_STEP.FINISH)
            {
                return _webService.End(ErrorCode.ERROR_INVALID_SPRINGCMAP_STEP);
            }
            else if (careerModeInfo.specialtraining_step != (byte)SPECIAL_TRAINING_STEP.NULL)
            {
                return _webService.End(ErrorCode.ERROR_INVALID_SPECIALTRAINING_STEP);
            }

            if(reqData.isSimulItemUse == true && dataSetWrapper.GetRowCount(1) == 0)
            {
                return _webService.End(ErrorCode.ERROR_NOT_HAVE_ITEM);
            }

            string battleKey = KeyGenerator.Instance.GetIncrementKey(GAME_KEY_TYPE.CAREER_MODE_GAME);
            _webService.WebSession.BattleKey = battleKey;


            // 결과 저장
            if (gameDB.USP_GS_GM_CAREERMODE_GAME_START(webSession.TokenInfo.Pcid, battleKey, simulItemUse) == false)
            {
                return _webService.End(ErrorCode.ERROR_DB, "USP_GS_GM_CAREERMODE_GAME_START");
            }
            return _webService.End();
        }
    }
}
