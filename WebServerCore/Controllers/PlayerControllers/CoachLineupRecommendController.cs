using System.Collections.Generic;
using System.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
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
    public class CoachLineupRecommendController : SessionContoller<ReqCoachLineupRecommend, ResCoachLineupRecommend>
    {
        public CoachLineupRecommendController(
            ILogger<CoachLineupRecommendController> logger,
            IConfiguration config, 
            IWebService<ReqCoachLineupRecommend, ResCoachLineupRecommend> webService, 
            IDBService dbService )
            : base( logger, config, webService, dbService )
        {
        }

        [HttpPost]
        [ApiExplorerSettings( GroupName = "client" )]
        [SwaggerExtend( "코치 추천 라인업", typeof( CoachLineupRecommendPacket ) )]
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

            string jsonCoachLineupList = JsonConvert.SerializeObject(reqData.CoachLineupList);

            DataSet dataSet = gameDB.USP_GS_GM_COACH_LINEUP_RECOMMEND_R(webSession.TokenInfo.Pcid, reqData.ModeType, jsonCoachLineupList);
            if (dataSet == null)
            {
                return _webService.End(ErrorCode.ERROR_DB, "USP_GS_GM_COACH_LINEUP_RECOMMEND_R");
            }

            DataSetWrapper dataSetWrapper = new DataSetWrapper(dataSet);
            List<AccountCoach> listCoach = dataSetWrapper.GetObjectList<AccountCoach>(0);
            List<AccountCoachSlot> listCoachSlot = dataSetWrapper.GetObjectList<AccountCoachSlot>(1);
            int coachSlotIdx = dataSetWrapper.GetValue<int>(2, "coach_slot_idx");
            
            if (listCoach.Count > coachSlotIdx || listCoach.Count <= 0)
            {
                return _webService.End(ErrorCode.ERROR_INVALID_SLOTIDX);
            }

            int reqCoachCnt = reqData.CoachLineupList.FindAll(x => x.account_coach_idx > 0).Count;

            if (reqCoachCnt != listCoach.Count)
            {
                return _webService.End(ErrorCode.ERROR_INVALID_COACH_DATA);
            }
            // 코치 슬롯별 포지션 제약 조건 확인
            ErrorCode recommendLineupResult = CacheManager.PBTable.PlayerTable.CheckRecommendCoachLineup(listCoach, listCoachSlot);
            if (recommendLineupResult != ErrorCode.SUCCESS)
            {
                return _webService.End(recommendLineupResult);
            }

            if (gameDB.USP_GS_GM_COACH_LINEUP_RECOMMEND(webSession.TokenInfo.Pcid, reqData.ModeType, jsonCoachLineupList) == false)
            {
                return _webService.End(ErrorCode.ERROR_DB, "USP_GS_GM_COACH_LINEUP_RECOMMEND");
            }

            return _webService.End();
        }
    }
}
