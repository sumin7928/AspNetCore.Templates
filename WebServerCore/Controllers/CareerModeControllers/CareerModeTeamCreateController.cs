using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Data;
using ApiWebServer.Cache;
using ApiWebServer.Common.Define;
using ApiWebServer.Core;
using ApiWebServer.Core.Controller;
using ApiWebServer.Core.Swagger;
using ApiWebServer.Database.Utils;
using ApiWebServer.PBTables;
using WebSharedLib.Contents;
using WebSharedLib.Contents.Api;
using WebSharedLib.Core.NPLib;
using WebSharedLib.Entity;
using WebSharedLib.Error;

namespace ApiWebServer.Controllers.CareerModeControllers
{
    [Route("api/CareerMode/[controller]")]
    [ApiController]
    public class CareerModeTeamCreateController : SessionContoller<ReqCareerModeTeamCreate, ResCareerModeTeamCreate>
    {
        public CareerModeTeamCreateController(
            ILogger<CareerModeTeamCreateController> logger,
            IConfiguration config, 
            IWebService<ReqCareerModeTeamCreate, ResCareerModeTeamCreate> webService, 
            IDBService dbService )
            : base( logger, config, webService, dbService )
        {
        }

        [HttpPost]
        [ApiExplorerSettings( GroupName = "client" )]
        [SwaggerExtend( "커리어모드 팀 생성", typeof(CareerModeTeamCreatePacket) )]
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

            DataSet dataSet = gameDB.USP_GS_GM_CAREERMODE_CREATE_TEAM_R(webSession.TokenInfo.Pcid, reqData.TeamIdx);
            if (dataSet == null)
            {
                return _webService.End(ErrorCode.ERROR_DB, "USP_GS_GM_CAREERMODE_CREATE_TEAM_R");
            }

            DataSetWrapper dataSetWrapper = new DataSetWrapper( dataSet );
            
            int db_teamIdx = dataSetWrapper.GetValue( 0, "team_idx", -1 );
            string jsonList = dataSetWrapper.GetValue( 1, "create_team_list", string.Empty );

            if ( jsonList == string.Empty )
            {
                return _webService.End( ErrorCode.ERROR_NOT_FOUND_CAREER_TEAM_DATA );
            }
            List<CareerModeCreateTeamInfo> createTeamList = JsonConvert.DeserializeObject<List<CareerModeCreateTeamInfo>>( jsonList );

            if ( db_teamIdx != 0 )
            {
                if ( db_teamIdx < 0 )
                {
                    return _webService.End( ErrorCode.ERROR_NOT_FOUND_CAREER_DATA );
                }
                else
                {
                    return _webService.End( ErrorCode.ERROR_ALREADY_TEAM_CREATED );
                }
            }

            // 팀 정보
            PB_TEAM_INFO teamInfo = CacheManager.PBTable.PlayerTable.GetTeamInfo( reqData.TeamIdx );
            if(teamInfo == null)
            {
                return _webService.End(ErrorCode.ERROR_STATIC_DATA);
            }
            if(false == CacheManager.PBTable.CareerModeTable.CreateTeam( reqData.TeamIdx, reqData.ModeLevel, out List<CareerModePlayer> players))
            {
                return _webService.End(ErrorCode.ERROR_STATIC_DATA);
            }

            // 저장된 팀정보에서 해당 팀 가져옴
            CareerModeCreateTeamInfo createTeamInfo = createTeamList.Find( x => x.team_idx == reqData.TeamIdx );
            if ( createTeamInfo == null )
            {
                return _webService.End( ErrorCode.ERROR_NOT_FOUND_CAREER_TEAM_DATA );
            }

            // 감독 목표 미션
            List<CareerModeMission> missionList = new List<CareerModeMission>();
            foreach( int mission_idx in createTeamInfo.mission_list )
            {
                missionList.Add( new CareerModeMission() { mission_idx = mission_idx } );
            }

            string jsonMissions = JsonConvert.SerializeObject( missionList );
            string jsonPlayers = JsonConvert.SerializeObject( players );
            byte halfType = (teamInfo.country_flg == (byte)NATION_LEAGUE_TYPE.CPB) ? (byte)1 : (byte)0;

            if ( gameDB.USP_GS_GM_CAREERMODE_CREATE_TEAM( webSession.TokenInfo.Pcid, jsonPlayers, reqData.TeamIdx, teamInfo, 
                halfType, reqData.ModeLevel, createTeamInfo.contract_no, jsonMissions,
                createTeamInfo.recommend_team_info ) == false)
            {
                return _webService.End( ErrorCode.ERROR_DB, "USP_GS_GM_CAREERMODE_CREATE_TEAM" );
            }

            resData.PlayerList = players;
            resData.MissionList = missionList;

            return _webService.End();
        }
    }
}
