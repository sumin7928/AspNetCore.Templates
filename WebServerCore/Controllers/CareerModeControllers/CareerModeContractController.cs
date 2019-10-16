using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Data;
using System.Linq;
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
    public class CareerModeContractController : SessionContoller<ReqCareerModeContract, ResCareerModeContract>
    {
        public CareerModeContractController(
            ILogger<CareerModeContractController> logger,
            IConfiguration config,
            IWebService<ReqCareerModeContract, ResCareerModeContract> webService,
            IDBService dbService )
            : base( logger, config, webService, dbService )
        {
        }

        [HttpPost]
        [ApiExplorerSettings( GroupName = "client" )]
        [SwaggerExtend( "커리어모드 감독 계약", typeof( CareerModeContractPacket ) )]
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
            var postDB = _dbService.CreatePostDB( _webService.RequestNo, webSession.DBNo );

            DataSet dataSet = gameDB.USP_GS_GM_CAREERMODE_CONTRACT_R(webSession.TokenInfo.Pcid);
            if (dataSet == null)
            {
                return _webService.End( ErrorCode.ERROR_DB, "USP_GS_GM_CAREERMODE_CONTRACT_R" );
            }

            DataSetWrapper dataSetWrapper = new DataSetWrapper( dataSet );
            CareerModeInfo careerInfo = dataSetWrapper.GetObject<CareerModeInfo>( 0 );
            AccountGame accountGameInfo = dataSetWrapper.GetObject<AccountGame>( 1 );

            // 유효성 체크
            if ( reqData.RequestContract == ( byte )CONTRACT_TYPE.RECONTRACT
                || reqData.RequestContract == ( byte )CONTRACT_TYPE.REJECT )
            {
                if( careerInfo.match_group != ( byte )SEASON_MATCH_GROUP.FINISHED )
                {
                    return _webService.End( ErrorCode.ERROR_NOT_FINISHED_CAREERMODE );
                }
                if ( careerInfo.previous_contract != ( byte )CONTRACT_TYPE.STAND_BY_CONTRACT )
                {
                    return _webService.End( ErrorCode.ERROR_NOT_BE_READY_FOR_CONTRACT );
                }

                // 재계약일 경우 계약 가능 여부 체크
                if( reqData.RequestContract == ( byte )CONTRACT_TYPE.RECONTRACT )
                {
                    List<CareerModeMission> missionList = dataSetWrapper.GetObjectList<CareerModeMission>( 2 );

                    if ( CacheManager.PBTable.CareerModeTable.IsCompleteMission( missionList ) == false )
                    {
                        return _webService.End( ErrorCode.ERROR_NOT_COMPLETE_CONTRACT_MISSION );
                    }
                }
            }
            else if( reqData.RequestContract == ( byte )CONTRACT_TYPE.FAIL )
            {
                if ( careerInfo.previous_contract != ( byte )CONTRACT_TYPE.STAND_BY_FAILED )
                {
                    return _webService.End( ErrorCode.ERROR_NOT_BE_READY_FOR_CONTRACT );
                }
            }
            else if( reqData.RequestContract == ( byte )CONTRACT_TYPE.RECONTRACT_REWARD )
            {
                if( careerInfo.recontract_cnt <= 0 )
                {
                    return _webService.End( ErrorCode.ERROR_NOT_FOUND_RECONTRACT_REWARD );
                }
            }

            // 계약 요청에 따른 처리
            ProcessContract( reqData.RequestContract, careerInfo, out List<GameRewardInfo> recontractRewardList, out List<CareerModeMission> newMissions );
            string newMissionJson = newMissions != null ? JsonConvert.SerializeObject(newMissions) : "";

            // 트렌젝션 처리 
            using( gameDB.BeginTransaction() )
            {
                if ( gameDB.USP_GS_GM_CAREERMODE_CONTRACT( webSession.TokenInfo.Pcid, JsonConvert.SerializeObject( careerInfo ), reqData.RequestContract, newMissionJson) == false )
                {
                    return _webService.End( ErrorCode.ERROR_DB, "USP_GS_GM_CAREERMODE_CONTRACT" );
                }

                // 우편함 보상 있을 경우
                if ( recontractRewardList != null )
                {
                    PostInsert postInsert = new PostInsert( webSession.PubId, recontractRewardList );

                    using ( postDB.BeginTransaction() )
                    {
                        if ( postDB.USP_GS_PO_POST_SEND( webSession.TokenInfo.Pcid, webSession.UserName, -1, "admin", postInsert, ( byte )POST_ADD_TYPE.ONE_BY_ONE ) == false )
                        {
                            return _webService.End( ErrorCode.ERROR_DB, "USP_GS_PO_POST_SEND" );
                        }
                        postDB.Commit();
                    }
                }
                gameDB.Commit();
            }

            resData.RecontractCount = careerInfo.recontract_cnt;
            resData.RecontractRewardIdx = recontractRewardList;

            return _webService.End();
        }

        private void ProcessContract( byte requestContract, CareerModeInfo careerInfo, out List<GameRewardInfo> recontractRewardList, out List<CareerModeMission> newMissions )
        {
            recontractRewardList = null;
            newMissions = null;

            // 재계약일 경우
            if ( requestContract == ( byte )CONTRACT_TYPE.RECONTRACT )
            {
                RecontractCareerData( requestContract, careerInfo );
                newMissions = new List<CareerModeMission>();
                List<int> missionList = CacheManager.PBTable.CareerModeTable.GetPennantraceMissionList();
                foreach (int mission_idx in missionList)
                {
                    newMissions.Add(new CareerModeMission() { mission_idx = mission_idx });
                }
            }
            // 재계약이 아닐경우 ( 거절, 실패, 파기 ) 
            else if ( requestContract == ( byte )CONTRACT_TYPE.REJECT ||
                requestContract == ( byte )CONTRACT_TYPE.FAIL ||
                requestContract == ( byte )CONTRACT_TYPE.DESTROY )
            {
                if ( careerInfo.recontract_cnt > 0 )
                {
                    recontractRewardList = CacheManager.PBTable.CareerModeTable.GetChainContractReward( careerInfo.country_type, careerInfo.recontract_cnt );
                }
                InitializeCareerData( requestContract, careerInfo );
            }
            // 재계약 보상일 경우
            else if ( requestContract == ( byte )CONTRACT_TYPE.RECONTRACT_REWARD )
            {
                recontractRewardList = CacheManager.PBTable.CareerModeTable.GetChainContractReward( careerInfo.country_type, careerInfo.recontract_cnt );
                careerInfo.recontract_cnt = 0;
            }
        }

        private void RecontractCareerData( byte contractNo, CareerModeInfo careerInfo )
        {
            careerInfo.total_career_cnt += 1;
            careerInfo.contract_no = 0;
            careerInfo.career_no += 1;
            careerInfo.mode_level = 0;
            careerInfo.country_type = ( byte )NATION_LEAGUE_TYPE.NONE;
            careerInfo.league_type = ( byte )LEAGUE_TYPE.NONE;
            careerInfo.area_type = ( byte )LEAGUE_AREA_TYPE.NONE;
            careerInfo.half_type = ( byte )SEASON_HALF_YEAR_TYPE.NONE;
            careerInfo.team_idx = 0;
            careerInfo.match_group = ( byte )SEASON_MATCH_GROUP.PENNANTRACE;
            careerInfo.match_type = ( byte )SEASON_MATCH_TYPE.NONE;
            careerInfo.degree_no = 1;
            careerInfo.game_no = 1;
            careerInfo.now_rank = 0;
            careerInfo.finish_match_group = ( byte )SEASON_MATCH_GROUP.NONE;
            careerInfo.recommend_buff_val = 0;

            careerInfo.recommend_reward_idx = 0;
            careerInfo.springcamp_step = ( byte )SPRING_CAMP_STEP.STEP_TRAINING;
            careerInfo.recontract_cnt += 1;
            careerInfo.previous_contract = contractNo;
            careerInfo.specialtraining_step = ( byte )SPECIAL_TRAINING_STEP.NULL;
            careerInfo.teammood = (short)CacheManager.PBTable.CareerModeTable.ManagementConfig.teammood_default;
        }

        private void InitializeCareerData( byte contractNo, CareerModeInfo careerInfo )
        {
            careerInfo.total_career_cnt += 1;
            careerInfo.contract_no = 0;
            careerInfo.career_no = 1;
            careerInfo.mode_level = 0;
            careerInfo.country_type = ( byte )NATION_LEAGUE_TYPE.NONE;
            careerInfo.league_type = ( byte )LEAGUE_TYPE.NONE;
            careerInfo.area_type = ( byte )LEAGUE_AREA_TYPE.NONE;
            careerInfo.half_type = ( byte )SEASON_HALF_YEAR_TYPE.NONE;
            careerInfo.team_idx = 0;
            careerInfo.match_group = ( byte )SEASON_MATCH_GROUP.PENNANTRACE;
            careerInfo.match_type = ( byte )SEASON_MATCH_TYPE.NONE;
            careerInfo.degree_no = 1;
            careerInfo.game_no = 1;
            careerInfo.now_rank = 0;
            careerInfo.finish_match_group = ( byte )SEASON_MATCH_GROUP.NONE;
            careerInfo.recommend_buff_val = 0;

            careerInfo.recommend_reward_idx = 0;
            careerInfo.springcamp_step = ( byte )SPRING_CAMP_STEP.STEP_TRAINING;
            careerInfo.last_rank = null;
            careerInfo.recontract_cnt = 0;
            careerInfo.previous_contract = contractNo;
            careerInfo.specialtraining_step = ( byte )SPECIAL_TRAINING_STEP.NULL;
            careerInfo.teammood = (short)CacheManager.PBTable.CareerModeTable.ManagementConfig.teammood_default;

        }
    }
}
