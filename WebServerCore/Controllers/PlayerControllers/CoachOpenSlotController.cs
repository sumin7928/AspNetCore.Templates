using System.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using ApiWebServer.Cache;
using ApiWebServer.Core;
using ApiWebServer.Core.Controller;
using ApiWebServer.Core.Swagger;
using ApiWebServer.Database.Utils;
using ApiWebServer.PBTables;
using WebSharedLib.Contents;
using WebSharedLib.Contents.Api;
using WebSharedLib.Core.NPLib;
using WebSharedLib.Error;

namespace ApiWebServer.Controllers.PlayerControllers
{
    [Route("api/Player/[controller]")]
    [ApiController]
    public class CoachOpenSlotController : SessionContoller<ReqCoachOpenSlot, ResCoachOpenSlot>
    {
        public CoachOpenSlotController(
            ILogger<CoachOpenSlotController> logger,
            IConfiguration config, 
            IWebService<ReqCoachOpenSlot, ResCoachOpenSlot> webService, 
            IDBService dbService )
            : base( logger, config, webService, dbService )
        {
        }

        [HttpPost]
        [ApiExplorerSettings( GroupName = "client" )]
        [SwaggerExtend( "코치 슬롯 오픈", typeof( CoachOpenSlotPacket ) )] 
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

            //오픈하려고 하는 pb 슬롯 데이터를 확인 후 유저 정보를 확인하여 재화를 보유중인지, 이미 열린 슬롯인지, 레벨 조건에 맞는지 체크한다.
            PB_COACH_SLOT_BASE coachSlotBaseInfo = CacheManager.PBTable.PlayerTable.GetCoachSlotBaseData(reqData.SlotIdx);
            if (coachSlotBaseInfo == null)
            {
                return _webService.End(ErrorCode.ERROR_NOT_MATCHING_PB_COACH_SLOT_BASE);
            }

            DataSet dataSet = gameDB.USP_GS_GM_COACH_OPEN_SLOT_R(webSession.TokenInfo.Pcid, coachSlotBaseInfo.coach_slot_open_cost_type);
            if (dataSet == null)
            {
                return _webService.End(ErrorCode.ERROR_DB, "USP_GS_GM_COACH_OPEN_SLOT_R");
            }

            DataSetWrapper dataSetWrapper = new DataSetWrapper(dataSet);
            int coachSlotIdx = dataSetWrapper.GetValue<int>(0, "coach_slot_idx");
            int userLv = dataSetWrapper.GetValue<int>(0, "user_lv");
            int currency = dataSetWrapper.GetValue<int>(0, "currency" );

            if (reqData.SlotIdx != coachSlotIdx + 1)
            {
                return _webService.End(ErrorCode.ERROR_INVALID_COACH_MAX_IDX);
            }

            if ( coachSlotBaseInfo.idx <= coachSlotIdx)
            {
                return _webService.End(ErrorCode.ERROR_INVALID_COACH_MAX_IDX);
            }

            if (coachSlotBaseInfo.coach_slot_open_cost_count > currency)
            {
                return _webService.End(ErrorCode.ERROR_NOT_ENOUGH_CURRENCY);
            }

            if (coachSlotBaseInfo.coach_slot_open_lv > userLv)
            {
                return _webService.End(ErrorCode.ERROR_NOT_ENOUGH_LV);
            }

            if (gameDB.USP_GS_GM_COACH_OPEN_SLOT(webSession.TokenInfo.Pcid, coachSlotBaseInfo) == false)
            {
                return _webService.End(ErrorCode.ERROR_DB, "USP_GS_GM_COACH_OPEN_SLOT");
            }
            resData.RstValue = currency - coachSlotBaseInfo.coach_slot_open_cost_count;

            return _webService.End();
        }
    }
}
