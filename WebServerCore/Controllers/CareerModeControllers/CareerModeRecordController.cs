using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Collections.Generic;
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

namespace ApiWebServer.Controllers.CareerModeControllers
{
    [Route( "api/CareerMode/[controller]" )]
    [ApiController]
    public class CareerModeRecordController : SessionContoller<ReqCareerModeRecord, ResCareerModeRecord>
    {
        public CareerModeRecordController(
            ILogger<CareerModeRecordController> logger,
            IConfiguration config,
            IWebService<ReqCareerModeRecord, ResCareerModeRecord> webService,
            IDBService dbService )
            : base( logger, config, webService, dbService )
        {
        }

        [HttpPost]
        [ApiExplorerSettings( GroupName = "client" )]
        [SwaggerExtend( "커리어모드 기록 데이터 조회", typeof( CareerModeRecordPacket ) ) ]
        public NPWebResponse Controller( [FromBody] NPWebRequest requestBody )
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

            // 커리어모드 기록 정보 조회
            DataSet dataSet = gameDB.USP_GS_GM_CAREERMODE_RECORD_R(webSession.TokenInfo.Pcid);
            if (dataSet == null)
            {
                return _webService.End( ErrorCode.ERROR_DB, "USP_GS_GM_CAREERMODE_RECORD_R" );
            }

            DataSetWrapper dataSetWrapper = new DataSetWrapper( dataSet );

            List<CareerModeHistory> histories = dataSetWrapper.GetObjectList<CareerModeHistory>( 0 );
            string peScore = dataSetWrapper.GetValue<string>( 1, "pe_score" );
            string peTeamMatch = dataSetWrapper.GetValue<string>( 1, "pe_team_match" );
            byte[] peGame = dataSetWrapper.GetValue<byte[]>( 1, "pe_game" );
            string poScore = dataSetWrapper.GetValue<string>( 1, "po_score" );
            string poTeamMatch = dataSetWrapper.GetValue<string>( 1, "po_team_match" );
            byte[] poGame = dataSetWrapper.GetValue<byte[]>( 1, "po_game" );

            CareerModeSeasonRecord seasonRecord = new CareerModeSeasonRecord()
            {
                pe_score = peScore,
                pe_team_match = peTeamMatch != null ? JsonConvert.DeserializeObject<Dictionary<string, string>>( peTeamMatch ) : null,
                pe_game = peGame != null ? MsgPack.Serialization.MessagePackSerializer.Get<Dictionary<string, CareerModeGameRecord>>().UnpackSingleObject( peGame ) : null,
                po_score = poScore,
                po_team_match = poTeamMatch != null ? JsonConvert.DeserializeObject<Dictionary<string, string>>( poTeamMatch ) : null,
                po_game = poGame != null ? MsgPack.Serialization.MessagePackSerializer.Get<Dictionary<string, CareerModeGameRecord>>().UnpackSingleObject( poGame ) : null
            };

            resData.CareerModeRecordList = histories;
            resData.CareerModeSeasonRecord = seasonRecord;

            return _webService.End();
        }
    }
}
