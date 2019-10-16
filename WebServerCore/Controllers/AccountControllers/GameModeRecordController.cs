using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Data;
using ApiWebServer.Core;
using ApiWebServer.Core.Controller;
using ApiWebServer.Core.Swagger;
using ApiWebServer.Database.Utils;
using WebSharedLib.Contents;
using WebSharedLib.Contents.Api;
using WebSharedLib.Core.NPLib;
using WebSharedLib.Entity;
using WebSharedLib.Error;

namespace ApiWebServer.Controllers.AccountControllers
{
    [Route( "api/Account/[controller]" )]
    [ApiController]
    public class GameModeRecordController : SessionContoller<ReqGameModeRecord, ResGameModeRecord>
    {
        public GameModeRecordController(
            ILogger<GameModeRecordController> logger,
            IConfiguration config,
            IWebService<ReqGameModeRecord, ResGameModeRecord> webService,
            IDBService dbService )
            : base( logger, config, webService, dbService )
        {
        }

        [HttpPost]
        [ApiExplorerSettings( GroupName = "client", IgnoreApi = true )]
        [SwaggerExtend( "감독 기록 조회", typeof( GameModeRecordPacket ) )]
        public NPWebResponse Contoller( [FromBody] NPWebRequest requestBody )
        {
            WrapWebService( requestBody );
            if ( _webService.ErrorCode != ErrorCode.SUCCESS )
            {
                return _webService.End( _webService.ErrorCode );
            }

            // Business
            var webSession = _webService.WebSession;
            var resData = _webService.WebPacket.ResData;
            var gameDB = _dbService.CreateGameDB( _webService.RequestNo, webSession.DBNo );

            DataSet dataSet = gameDB.USP_GS_GM_ACCOUNT_MODE_RECORD_R(webSession.TokenInfo.Pcid);
            if ( dataSet == null )
            {
                return _webService.End( ErrorCode.ERROR_DB, "USP_GS_GM_ACCOUNT_MODE_RECORD_R" );
            }

            DataSetWrapper dataSetWrapper = new DataSetWrapper( dataSet );
            resData.BattleModeRecordList = dataSetWrapper.GetObjectList<BattleModeHistory>( 0 );
            resData.CareerModeRecordList = dataSetWrapper.GetObjectList<CareerModeHistory>( 1 );

            return _webService.End();
        }
    }
}
