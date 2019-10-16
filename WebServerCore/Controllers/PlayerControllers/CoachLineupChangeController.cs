using System.Collections.Generic;
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
    public class CoachLineupChangeController : SessionContoller<ReqCoachLineupChange, ResCoachLineupChange>
    {
        public CoachLineupChangeController(
            ILogger<CoachLineupChangeController> logger,
            IConfiguration config, 
            IWebService<ReqCoachLineupChange, ResCoachLineupChange> webService, 
            IDBService dbService )
            : base( logger, config, webService, dbService )
        {
        }

        [HttpPost]
        [ApiExplorerSettings( GroupName = "client" )]
        [SwaggerExtend( "코치 라인업 변경", typeof( CoachLineupChangePacket ) )]
        public NPWebResponse Controller( [FromBody] NPWebRequest requestBody )
        {
            WrapWebService( requestBody );
            if ( _webService.ErrorCode != ErrorCode.SUCCESS)
            {
                return _webService.End( _webService.ErrorCode );
            }

            // Business
            var webSession = _webService.WebSession;
            var reqData = _webService.WebPacket.ReqData;
            var resData = _webService.WebPacket.ResData;
            var gameDB = _dbService.CreateGameDB(_webService.RequestNo, webSession.DBNo);

            if ( reqData.SrcAccountCoachIdx == reqData.DstAccountCoachIdx )
            {
                return _webService.End( ErrorCode.ERROR_INVALID_PARAM );
            }

            DataSet dataSet = gameDB.USP_GS_GM_COACH_LINEUP_CHANGE_R(webSession.TokenInfo.Pcid, reqData.ModeType, reqData.CoachSlotIdx, reqData.SrcAccountCoachIdx, reqData.DstAccountCoachIdx);
            if (dataSet == null)
            {
                return _webService.End(ErrorCode.ERROR_DB, "USP_GS_GM_COACH_LINEUP_CHANGE_R");
            }

            DataSetWrapper dataSetWrapper = new DataSetWrapper(dataSet);
            List<AccountCoach> listCoach = dataSetWrapper.GetObjectList<AccountCoach>(0);
            int coachPosition = dataSetWrapper.GetValue<int>(1, "position");

            AccountCoach mainCoach = null;
            AccountCoach subCoach = null;

            //코치 있는지 체크
            if ( listCoach == null )
            {
                return _webService.End( ErrorCode.ERROR_INVALID_LINEUP_LIST );
            }

            // 코치 등록 및 해제
            if ( reqData.CoachSlotIdx > 0 )
            {
                // 요청 값 검증
                if ( reqData.SrcAccountCoachIdx > 0 && reqData.DstAccountCoachIdx > 0 )
                {
                    return _webService.End( ErrorCode.ERROR_INVALID_COACH_DATA );
                }

                // 등록
                if ( reqData.SrcAccountCoachIdx > 0 )
                {
                    ErrorCode inputResult = CacheManager.PBTable.PlayerTable.InputCoach( reqData, listCoach, coachPosition, out mainCoach, out subCoach );
                    if ( inputResult != ErrorCode.SUCCESS )
                    {
                        return _webService.End( inputResult );
                    }
                }
                // 해제
                else if ( reqData.DstAccountCoachIdx > 0 )
                {
                    ErrorCode outputResult = CacheManager.PBTable.PlayerTable.OutputCoach( reqData, listCoach, out mainCoach, out subCoach );
                    if ( outputResult != ErrorCode.SUCCESS )
                    {
                        return _webService.End( outputResult );
                    }
                }
                else
                {
                    return _webService.End( ErrorCode.ERROR_INVALID_COACH_DATA );
                }
            }
            // 코치 라인업 교체
            else
            {
                ErrorCode changeLineupResult = CacheManager.PBTable.PlayerTable.ChangeCoachLineup( reqData, listCoach, out mainCoach, out subCoach );
                if ( changeLineupResult != ErrorCode.SUCCESS )
                {
                    return _webService.End( changeLineupResult );
                }
            }

            if (gameDB.USP_GS_GM_COACH_LINEUP_CHANGE(webSession.TokenInfo.Pcid, reqData.ModeType, mainCoach.account_coach_idx, subCoach.account_coach_idx, reqData.CoachSlotIdx) == false)
            {
                return _webService.End(ErrorCode.ERROR_DB, "USP_GS_GM_COACH_LINEUP_CHANGE");
            }

            return _webService.End();
        }
    }
}
