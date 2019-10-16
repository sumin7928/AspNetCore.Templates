using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MsgPack.Serialization;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Data;
using ApiWebServer.Cache;
using ApiWebServer.Common.Define;
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
    [Route( "api/CareerMode/[controller]" )]
    [ApiController]
    public class CareerModeGameSkipController : SessionContoller<ReqCareerModeGameSkip, ResCareerModeGameSkip>
    {
        public CareerModeGameSkipController(
            ILogger<CareerModeGameSkipController> logger,
            IConfiguration config,
            IWebService<ReqCareerModeGameSkip, ResCareerModeGameSkip> webService,
            IDBService dbService )
            : base( logger, config, webService, dbService )
        {
        }

        [HttpPost]
        [ApiExplorerSettings( GroupName = "client")]
        [SwaggerExtend( "커리어모드 타팀 차수 스킵 처리", typeof( CareerModeGameSkipPacket ) )]
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

            // 기본 정보 조회
            DataSet dataSet = gameDB.USP_GS_GM_CAREERMODE_SKIP_R(webSession.TokenInfo.Pcid);
            if (dataSet == null)
            {
                return _webService.End( ErrorCode.ERROR_DB, "USP_GS_GM_CAREERMODE_SIMUL_END_R" );
            }

            DataSetWrapper dataSetWrapper = new DataSetWrapper( dataSet );
            CareerModeInfo careerModeInfo = dataSetWrapper.GetObject<CareerModeInfo>( 0 );

            // 현재 커리어 모드 검증
            if ( reqData.CareerNo != careerModeInfo.career_no ||
                reqData.MatchGroup != careerModeInfo.match_group ||
                reqData.NextDegreeNo <= careerModeInfo.degree_no )
            {
                return _webService.End( ErrorCode.ERROR_NOT_MATCHING_INFO );
            }

            if (reqData.FinishMatchGroup > 0 )
            {
                if (reqData.MatchGroup == (byte)SEASON_MATCH_GROUP.PENNANTRACE)
                {
                    careerModeInfo.finish_match_group = (byte)SEASON_MATCH_GROUP.PENNANTRACE;
                }
                else
                {
                    careerModeInfo.finish_match_group = (byte)SEASON_MATCH_GROUP.POST_SEASON;
                }
            }

            careerModeInfo.degree_no = reqData.NextDegreeNo;
            careerModeInfo.match_type = reqData.MatchType;
            careerModeInfo.now_rank = reqData.Rank;
            string matchTeamRecord = JsonConvert.SerializeObject(reqData.MatchTeamRecord);
            byte[] gameRecord = MessagePackSerializer.Get<Dictionary<string, CareerModeGameRecord>>().PackSingleObject(reqData.GameRecord);

            // 결과 저장
            if ( gameDB.USP_GS_GM_CAREERMODE_SKIP( webSession.TokenInfo.Pcid, careerModeInfo, matchTeamRecord, gameRecord) == false )
            {
                return _webService.End( ErrorCode.ERROR_DB, "USP_GS_GM_CAREERMODE_SIMUL_END" );
            }

            return _webService.End();
        }
    }
}
