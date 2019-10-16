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
using ApiWebServer.Models;
using WebSharedLib.Contents;
using WebSharedLib.Contents.Api;
using WebSharedLib.Core.NPLib;
using WebSharedLib.Entity;
using WebSharedLib.Error;

namespace ApiWebServer.Controllers.CareerModeControllers
{
    [Route("api/CareerMode/[controller]")]
    [ApiController]
    public class CareerModeInfoController : SessionContoller<ReqCareerModeInfo, ResCareerModeInfo>
    {
        public CareerModeInfoController(
            ILogger<CareerModeInfoController> logger,
            IConfiguration config, 
            IWebService<ReqCareerModeInfo, ResCareerModeInfo> webService, 
            IDBService dbService )
            : base( logger, config, webService, dbService )
        {
        }

        [HttpPost]
        [ApiExplorerSettings( GroupName = "client" )]
        [SwaggerExtend( "커리어모드 인포", typeof( CareerModeInfoPacket ) )]
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

            DataSet dataSet = gameDB.USP_GS_GM_CAREERMODE_INFO_R(webSession.TokenInfo.Pcid);
            if (dataSet == null)
            {
                return _webService.End(ErrorCode.ERROR_DB, "USP_GS_GM_CAREERMODE_INFO_R");
            }

            List<CareerModeCreateTeamInfo> createTeamList = null;
            List<CareerModeCycleEventInfo> newEventList = null;
            DataSetWrapper dataSetWrapper = new DataSetWrapper(dataSet);
            CareerModeInfo careerInfo = dataSetWrapper.GetObject<CareerModeInfo>(0);

            //커리어모드 최초입장시
            if ( careerInfo == null)
            {
                careerInfo = CreateCareerMode();

                // 팀 정보
                createTeamList = CacheManager.PBTable.CareerModeTable.CreateTeamList( webSession.NationType );

                if (createTeamList.Count == 0)
                {
                    return _webService.End(ErrorCode.ERROR_STATIC_DATA, "Not found created team list");
                }

                if ( gameDB.USP_GS_GM_CAREERMODE_INFO( webSession.TokenInfo.Pcid, 
                    JsonConvert.SerializeObject( careerInfo ), 
                    JsonConvert.SerializeObject( createTeamList ) ) == false )
                {
                    return _webService.End( ErrorCode.ERROR_DB, "USP_GS_GM_CAREERMODE_INFO" );
                }
            }
            else
            {
                //팀선택을 안했을경우
                if ( careerInfo.team_idx == 0)
                {
                    string jsonList = dataSetWrapper.GetValue( 1, "create_team_list", string.Empty );

                    if ( jsonList == string.Empty )
                    {
                        // 팀 정보
                        createTeamList = CacheManager.PBTable.CareerModeTable.CreateTeamList( webSession.NationType, careerInfo.previous_contract );

                        if ( createTeamList.Count == 0)
                        {
                            return _webService.End( ErrorCode.ERROR_STATIC_DATA, "Not found created team list" );
                        }

                        if (gameDB.USP_GS_GM_CAREERMODE_INFO( webSession.TokenInfo.Pcid, string.Empty, JsonConvert.SerializeObject(createTeamList)) == false)
                        {
                            return _webService.End(ErrorCode.ERROR_DB, "USP_GS_GM_CAREERMODE_INFO");
                        }
                    }
                    else
                    {
                        createTeamList = JsonConvert.DeserializeObject<List<CareerModeCreateTeamInfo>>( jsonList );
                    }
                }
                else if(careerInfo.event_flag == (byte)CYCLE_EVENT_FLAG.NEW_CYCLE_NEW_EVENT)
                {
                    newEventList = dataSetWrapper.GetObjectList<CareerModeCycleEventInfo>(1);
                }
            }

            resData.TotalCareerCnt = careerInfo.total_career_cnt;
            resData.ContractNo = careerInfo.contract_no;
            resData.CareerNo = careerInfo.career_no;
            resData.ModeLevel = careerInfo.mode_level;
            resData.CountryType = careerInfo.country_type;
            resData.LeagueType = careerInfo.league_type;
            resData.AreaType = careerInfo.area_type;
            resData.HalfType = careerInfo.half_type;
            resData.TeamIdx = careerInfo.team_idx;
            resData.MatchGroup = careerInfo.match_group;
            resData.MatchType = careerInfo.match_type;
            resData.DegreeNo = careerInfo.degree_no;
            resData.GameNo = careerInfo.game_no;
            resData.RecommendBuffVal = careerInfo.recommend_buff_val;
            resData.SpringcampStep = careerInfo.springcamp_step;
            resData.LastRank = careerInfo.last_rank;
            resData.RecontractCount = careerInfo.recontract_cnt;
            resData.PreviousContract = careerInfo.previous_contract;
            resData.RecommendRewardIdx = careerInfo.recommend_reward_idx;
            resData.CreateTeamList = createTeamList;
            resData.SpecialTrainingStep = careerInfo.specialtraining_step;
            resData.IsNewCycleEvent = (careerInfo.event_flag > (byte)CYCLE_EVENT_FLAG.NOT_CYCLE) ? true : false;
            resData.NewCycleEventList = newEventList;
            //resData.TeamMood = careerInfo.teammood;
            return _webService.End();
        }

        private CareerModeInfo CreateCareerMode()
        {
            //커리어모드 최초입장시
            CareerModeInfo careerInfo = new CareerModeInfo()
            {
                total_career_cnt = 1,
                contract_no = 0,
                career_no = 1,
                mode_level = 0,
                country_type = ( byte )NATION_LEAGUE_TYPE.NONE,
                league_type = ( byte )LEAGUE_TYPE.NONE,
                area_type = ( byte )LEAGUE_AREA_TYPE.NONE,
                half_type = ( byte )SEASON_HALF_YEAR_TYPE.NONE,
                team_idx = 0,
                match_group = ( byte )SEASON_MATCH_GROUP.PENNANTRACE,
                match_type = ( byte )SEASON_MATCH_TYPE.NONE,
                degree_no = 1,
                game_no = 1,
                now_rank = 0,
                finish_match_group = ( byte )SEASON_MATCH_GROUP.NONE,
                recommend_buff_val = 0,
                springcamp_step = (byte)SPRING_CAMP_STEP.STEP_TRAINING,
                previous_contract = 0,
                recontract_cnt = 0,
                recommend_reward_idx = 0,
                specialtraining_step = (byte)SPECIAL_TRAINING_STEP.NULL,
                teammood = (short)CacheManager.PBTable.CareerModeTable.ManagementConfig.teammood_default
            };

            return careerInfo;
        }
    }
}
