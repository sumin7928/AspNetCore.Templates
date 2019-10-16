using System;
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
using ApiWebServer.PBTables;
using WebSharedLib.Contents;
using WebSharedLib.Contents.Api;
using WebSharedLib.Core.NPLib;
using WebSharedLib.Entity;
using WebSharedLib.Error;

namespace ApiWebServer.Controllers.PlayerControllers
{
    [Route("api/Player/[controller]")]
    [ApiController]
    public class TeamCreateController : SessionContoller<ReqTeamCreate, ResTeamCreate>
    {
        public TeamCreateController(
            ILogger<TeamCreateController> logger,
            IConfiguration config, 
            IWebService<ReqTeamCreate, ResTeamCreate> webService, 
            IDBService dbService )
            : base( logger, config, webService, dbService )
        {
        }

        [HttpPost]
        [ApiExplorerSettings( GroupName = "client" )]
        [SwaggerExtend( "팀 생성", typeof( TeamCreatePacket ) )]
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

            DataSet dataSet = gameDB.USP_GS_GM_PLAYER_TEAMCREATE_R(webSession.TokenInfo.Pcid);
            if (dataSet == null)
            {
                return _webService.End(ErrorCode.ERROR_DB, "USP_GS_GM_PLAYER_TEAMCREATE_R");
            }

            DataSetWrapper dataSetWrapper = new DataSetWrapper(dataSet);
            int teamIdx = dataSetWrapper.GetValue<int>(0, "team_idx");

            if(teamIdx > 0)
            {
                return _webService.End(ErrorCode.ERROR_ALREADY_TEAM_CREATE);
            }

            ErrorCode createNewTeamResult;
            //라이브 모드 랜덤 선수 지급 기능 막고 예전처럼 해당 팀에 대한 유저 지급으로 변경. 일단 주석으로 나두자.
            //createNewTeamResult = CacheManager.PBTable.PlayerTable.CreateNewTeam(reqData.TeamIdx, out List<Player> players, out List<TeamCoach> coachs, reqData.PitcherIdx, reqData.BatterIdx, reqData.CoachIdx);
            createNewTeamResult = CacheManager.PBTable.PlayerTable.CreateTeam(reqData.TeamIdx, out List<Player> players, out List<TeamCoach> coachs);
            if (createNewTeamResult != ErrorCode.SUCCESS)
            {
                return _webService.End(createNewTeamResult);
            }
            
            if ( players == null || coachs == null )
            {
                return _webService.End( ErrorCode.ERROR_STATIC_DATA );
            }
            Dictionary<int, int> tempLineupMaxValue = new Dictionary<int, int>();

            // 팀테이블에서 가져온 선수들이 제약 조건에 맞는지 체크
            foreach (TeamCoach c in coachs)
            {
                if (c.position > 0)
                {
                    PB_COACH_POSITION coachPositionInfo = CacheManager.PBTable.PlayerTable.GetCoachPositionData(c.position);
                    PB_COACH coachPBInfo = CacheManager.PBTable.PlayerTable.GetCoachData(c.coach_idx);
                    string[] tempWords = coachPositionInfo.master_position_num.Split('|');
                    if (Array.Exists(tempWords, e => e == coachPBInfo.master_position.ToString()) == false)
                    {
                        return _webService.End(ErrorCode.ERROR_NOT_MATCHING_COACH_POSITION);
                    }
                    if (tempLineupMaxValue.ContainsKey(coachPositionInfo.idx) == true)
                    {
                        tempLineupMaxValue[coachPositionInfo.idx] += 1;
                    }
                    else
                    {
                        tempLineupMaxValue.Add(coachPositionInfo.idx, 1);
                    }
                    if (tempLineupMaxValue[coachPositionInfo.idx] > coachPositionInfo.lineup_max_value)
                    {
                        return _webService.End(ErrorCode.ERROR_NOT_MATCHING_COACH_POSITION);
                    }
                }
            }

            string jsonPlayers = JsonConvert.SerializeObject( players );
            string jsonCoachs = JsonConvert.SerializeObject( coachs );

            Dictionary<int, int> coachSlotList = CacheManager.PBTable.PlayerTable.GetDefaultCoachSlotPosition();
            string jsonDefaultCoachSlot = JsonConvert.SerializeObject(coachSlotList);

            if (gameDB.USP_GS_GM_PLAYER_TEAMCREATE(webSession.TokenInfo.Pcid, jsonPlayers, jsonCoachs, jsonDefaultCoachSlot, reqData.TeamIdx) == false)
            {
                return _webService.End(ErrorCode.ERROR_DB, "USP_GS_GM_PLAYER_TEAMCREATE");
            }

            webSession.TeamIdx = reqData.TeamIdx;

            return _webService.End();
        }
    }
}
