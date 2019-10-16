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

namespace ApiWebServer.Controllers.ScoutControllers
{
    [Route("api/Scout/[controller]")]
    [ApiController]
    public class ScoutInfoController : SessionContoller<ReqScoutInfo, ResScoutInfo>
    {
        public ScoutInfoController(
            ILogger<ScoutInfoController> logger,
            IConfiguration config,
            IWebService<ReqScoutInfo, ResScoutInfo> webService, 
            IDBService dbService )
            : base( logger, config, webService, dbService )
        {
        }

        [HttpPost]
        [ApiExplorerSettings( GroupName = "client" )]
        [SwaggerExtend( "스카우트 인포", typeof(ScoutInfoPacket) )]
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
            var gameDB = _dbService.CreateGameDB(_webService.RequestNo, webSession.DBNo);

            byte isCreate = 0;

            // 스카우트 정보 조회
            DataSet dataSet = gameDB.USP_GS_GM_SCOUT_INFO_R(webSession.TokenInfo.Pcid);
            if (dataSet == null)
            {
                return _webService.End( ErrorCode.ERROR_DB, "USP_GS_GM_SCOUT_INFO_R");
            }

            DataSetWrapper dataSetWrapper = new DataSetWrapper( dataSet );

            if(dataSetWrapper.GetRowCount(0) == 0)
            {
                return _webService.End(ErrorCode.ERROR_STATIC_DATA, "scout_binder_static_data");
            }

            //바인더넘버, 갱신까지 남은시간
            int dateNo = dataSetWrapper.GetValue<int>(0, "no");
            int remainSec = dataSetWrapper.GetValue<int>(0, "remain_sec");

            if(remainSec < 0)
            {
                return _webService.End(ErrorCode.ERROR_DB_DATA, "remainSec error");
            }

            AccountScoutBinder scoutBinderInfo = dataSetWrapper.GetObject<AccountScoutBinder>(1);
            List<AccountScoutSlot> scoutSlotList = dataSetWrapper.GetObjectList<AccountScoutSlot>(2);

            //영입 최초 접속
            if(scoutBinderInfo == null)
            {
                isCreate = 1;

                scoutBinderInfo = new AccountScoutBinder();
            }

            //최초접속이거나 넘버가 바뀌면 새로운 바인더로 셋팅
            if(isCreate == 1 || scoutBinderInfo.date_no != dateNo)
            {
                ErrorCode errorCode = Cache.CacheManager.PBTable.ItemTable.SetScoutBinderInfo(scoutBinderInfo, dateNo, webSession.NationType, false);
                if(errorCode != ErrorCode.SUCCESS)
                {
                    return _webService.End(errorCode, "SetScoutBinderInfo Error");
                }

                if (gameDB.USP_GS_GM_SCOUT_INFO(webSession.TokenInfo.Pcid, isCreate, scoutBinderInfo) == false)
                {
                    return _webService.End(ErrorCode.ERROR_DB, "USP_GS_GM_SCOUT_INFO");
                }
            }

            resData.BinderResetRemainSec = remainSec;
            resData.UserBinderInfo = scoutBinderInfo;
            resData.UserProcessSlotInfo = scoutSlotList;


            return _webService.End();
        }
    }
}
