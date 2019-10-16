using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using ApiWebServer.Cache;
using ApiWebServer.Common.Define;
using ApiWebServer.Core;
using ApiWebServer.Core.Controller;
using ApiWebServer.Core.Swagger;
using ApiWebServer.Logic;
using ApiWebServer.Database.Utils;
using WebSharedLib.Contents;
using WebSharedLib.Contents.Api;
using WebSharedLib.Core.NPLib;
using WebSharedLib.Entity;
using WebSharedLib.Error;

namespace ApiWebServer.Controllers.AccountControllers
{
    [Route("api/Account/[controller]")]
    [ApiController]
    public class CheckAccountInfoController : SessionContoller<ReqCheckAccountInfo, ResCheckAccountInfo>
    {
        public CheckAccountInfoController( 
            ILogger<CheckAccountInfoController> logger,
            IConfiguration config, 
            IWebService<ReqCheckAccountInfo, ResCheckAccountInfo> webService, 
            IDBService dbService )
            : base( logger, config, webService, dbService )
        {
        }

        [HttpPost]
        [ApiExplorerSettings( GroupName = "client" )]
        [SwaggerExtend( "계정관련 정보 업데이트 체크", typeof(CheckAccountInfoPacket) )]
        public NPWebResponse Contoller([FromBody] NPWebRequest requestBody )
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


            DataSet gameDataSet = gameDB.USP_GS_GM_ACCOUNT_CHECK_R(webSession.TokenInfo.Pcid);
            if (gameDataSet == null)
            {
                return _webService.End(ErrorCode.ERROR_DB, "USP_GS_GM_ACCOUNT_CHECK_R");
            }

            DataSetWrapper gameDataSetWrapper = new DataSetWrapper(gameDataSet);

            if (gameDataSetWrapper.GetRowCount(0) == 0)
            {
                return _webService.End(ErrorCode.ERROR_STATIC_DATA, "not db day week idx ");
            }

            int dayIdx = gameDataSetWrapper.GetValue<int>(0, "day_idx");
            int dayType = gameDataSetWrapper.GetValue<int>(0, "day_type");
            int weekIdx = gameDataSetWrapper.GetValue<int>(0, "week_idx");
          
            AccountGame accountGameInfo = gameDataSetWrapper.GetObject<AccountGame>(1);
            List<RepeatMission> missionList = gameDataSetWrapper.GetObjectList<RepeatMission>(2);
            List<Achievement> achivementList = gameDataSetWrapper.GetObjectList<Achievement>(3);

            List<RepeatMission> newMissionList = null;
            List<Achievement> newAchivementList = null;

            bool isDayChange = false;
            string deleteMissionType = "";

            if(dayIdx != accountGameInfo.day_idx)
            {
                isDayChange = true;
                newMissionList = new List<RepeatMission>();

                deleteMissionType += (byte)MISSION_TYPE_DB.DAY;
                CacheManager.PBTable.MissionAchievementTable.AddMissionDay(newMissionList, dayType);

                missionList.RemoveAll(x => x.type == (byte)MISSION_TYPE_DB.DAY);

                if (weekIdx != accountGameInfo.week_idx)
                {

                    deleteMissionType += "," + (byte)MISSION_TYPE_DB.WEEK;
                    CacheManager.PBTable.MissionAchievementTable.AddMissionWeek(newMissionList);

                    missionList.RemoveAll(x => x.type == (byte)MISSION_TYPE_DB.WEEK);
                }

                missionList.AddRange(newMissionList);
            }

            //새로운 업적있는지체크(날짜바뀌었을때만)
            newAchivementList = CacheManager.PBTable.MissionAchievementTable.GetNewAchievement(webSession.NationType, achivementList);
            if (newAchivementList != null)
                achivementList.AddRange(newAchivementList);


            if (gameDB.USP_GS_GM_ACCOUNT_CHECK(webSession.TokenInfo.Pcid, isDayChange, dayIdx, weekIdx, deleteMissionType, newMissionList, newAchivementList) == false)
            {
                return _webService.End(ErrorCode.ERROR_DB, "USP_GS_GM_ACCOUNT_CHECK");
            }

            //미션은 미완료만 캐싱해놓기
            webSession.MissionList = CacheManager.PBTable.MissionAchievementTable.GetUserMissionList(missionList);
            //업적은 맥스치까지 끝나지않은것 캐싱
            webSession.AchievementList = CacheManager.PBTable.MissionAchievementTable.GetUserAchievementList(achivementList);

            resData.MissionList = missionList;
            resData.AchievementList = achivementList;

            return _webService.End();
        }
    }
}
