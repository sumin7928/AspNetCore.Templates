using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Data;
using System.Text;
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
    public class CareerModeSpringCampSetController : SessionContoller<ReqCareerModeSpringCampSet, ResCareerModeSpringCampSet>
    {
        public CareerModeSpringCampSetController(
            ILogger<CareerModeSpringCampSetController> logger,
            IConfiguration config, 
            IWebService<ReqCareerModeSpringCampSet, ResCareerModeSpringCampSet> webService, 
            IDBService dbService )
            : base( logger, config, webService, dbService )
        {
        }

        [HttpPost]
        [ApiExplorerSettings( GroupName = "client" )]
        [SwaggerExtend( "커리어모드 스프링캠프 셋팅", typeof(CareerModeSpringCampSetPacket) )]
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

            List<long> trainingPlayerList = new List<long>();
            string trainingPlayerStr = string.Empty;
            byte allFinish = 0;

            // 그룹 훈련일 경우 선수 정보 미리 저장
            if ( reqData.Step == (byte)SPRING_CAMP_STEP.STEP_TRAINING)
            {
                foreach( var info in reqData.TrainingInfo )
                {
                    trainingPlayerList.AddRange( info.PlayerSerials );
                }

                trainingPlayerStr = Common.ServerUtils.MakeSplittedString( trainingPlayerList );

                if ( trainingPlayerList.Count == 0)
                {
                    return _webService.End(ErrorCode.ERROR_INVALID_PARAM);
                }
            }

            DataSet dataSet = gameDB.USP_GS_GM_CAREERMODE_SPRINGCAMP_SET_R(webSession.TokenInfo.Pcid, reqData.Step, trainingPlayerStr);
            if (dataSet == null)
            {
                return _webService.End(ErrorCode.ERROR_DB, "USP_GS_GM_CAREERMODE_SPRINGCAMP_SET_R");
            }

            DataSetWrapper dataSetWrapper = new DataSetWrapper(dataSet);
            byte career_no = dataSetWrapper.GetValue<byte>(0, "career_no");
            byte mode_level = dataSetWrapper.GetValue<byte>(0, "mode_level");
            byte springcamp_step = dataSetWrapper.GetValue<byte>(0, "springcamp_step");
            List<CareerModeSpringCamp> springCampSetList = dataSetWrapper.GetObjectList<CareerModeSpringCamp>( 1 );

            // 유효성 체크
            if (reqData.CareerNo != career_no ) 
            {
                return _webService.End(ErrorCode.ERROR_NOT_MATCHING_INFO);
            }
            else if( mode_level == 0 )
            {
                return _webService.End( ErrorCode.ERROR_NOT_FOUND_MODE_LEVEL );
            }
            else if( springcamp_step != ( byte )SPRING_CAMP_STEP.STEP_TRAINING )
            {
                return _webService.End( ErrorCode.ERROR_INVALID_SPRINGCMAP_STEP );
            }

            //해당 캠프타입의 전체 리스트
            List<CareerModeSpringCamp> resultSpringCampInfo = new List<CareerModeSpringCamp>();
            List<AccountPlayerTrainingInfo> potenPlayerInfo = new List<AccountPlayerTrainingInfo>();

            if (reqData.Step == (byte)SPRING_CAMP_STEP.STEP_TRAINING)
            {
                List<Player> players = dataSetWrapper.GetObjectList<Player>(2);
                List<long> alreadyPlayerList = new List<long>();

                //중복을 막기위한 체크(요청한 선수수와 db에 있는 중복제거된 선수 수가 일치하지 않으면 에러)
                if( trainingPlayerList.Count != players.Count)
                {
                    return _webService.End(ErrorCode.ERROR_NOT_MATCHING_INFO);
                }

                //해당 타입의 훈련을 한 선수 리스트로 저장
                foreach (CareerModeSpringCamp campInfo in springCampSetList)
                {
                    string[] playerDetailInfo = campInfo.detail_info.Split("|");
                    for (int i = 0; i < playerDetailInfo.Length; ++i)
                    {
                        alreadyPlayerList.Add(long.Parse(playerDetailInfo[i].Split("/")[0]));
                    }
                }

                foreach (CareerModeTrainingInfo info in reqData.TrainingInfo)
                {
                    //같은 훈련을 이미 했는지 체크
                    if (springCampSetList.Find(x => x.training_type == info.training_id) != null)
                    {
                        return _webService.End(ErrorCode.ERROR_ALREADY_SPRINGCAMP_TRAINING);
                    }

                    List<Player> playerInfo = players.FindAll(x => info.PlayerSerials.Contains(x.account_player_idx));

                    //요청한 선수와 DB에 있는 선수가 맞지 않다면 에러
                    if (info.PlayerSerials.Count != playerInfo.Count)
                    {
                        return _webService.End(ErrorCode.ERROR_INVALID_PARAM, "springcamp set player count not match");
                    }

                    ErrorCode errCode = CacheManager.PBTable.CareerModeTable.SpringCampSetTraining(info.training_id, playerInfo, alreadyPlayerList, ref resultSpringCampInfo, ref potenPlayerInfo);

                    if (errCode != ErrorCode.SUCCESS)
                    {
                        return _webService.End(errCode, "springcamp set player count not match");
                    }
                }
            }
            else if (reqData.Step == (byte)SPRING_CAMP_STEP.STEP_TEAM_BONUS)
            {
                //보너스는 한번만 받으므로 이미 받았다면 에러
                if(springCampSetList.Count != 0)
                {
                    return _webService.End(ErrorCode.ERROR_ALREADY_SPRINGCAMP_TRAINING);
                }

                ErrorCode errCode = CacheManager.PBTable.CareerModeTable.SpringCampSetBonus(ref resultSpringCampInfo);

                if (errCode != ErrorCode.SUCCESS)
                {
                    return _webService.End(errCode, "springcamp set player count not match");
                }

                allFinish = 1;
            }
            else
            {
                return _webService.End(ErrorCode.ERROR_NOT_MATCHING_INFO);
            }

            if ( gameDB.USP_GS_GM_CAREERMODE_SPRINGCAMP_SET( webSession.TokenInfo.Pcid, reqData.Step, resultSpringCampInfo, potenPlayerInfo, allFinish) == false)
            {
                return _webService.End( ErrorCode.ERROR_DB, "USP_GS_GM_CAREERMODE_SPRINGCAMP_SET");
            }

            resData.ResultSpringCampInfo = resultSpringCampInfo;
            resData.AllStepFinish = allFinish;

            return _webService.End();
        }
    }
}
