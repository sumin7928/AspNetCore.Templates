using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Data;
using ApiWebServer.Cache;
using ApiWebServer.Core;
using ApiWebServer.Core.Swagger;
using WebSharedLib.Contents;
using WebSharedLib.Core.NPLib;
using WebSharedLib.Error;
using WebSharedLib.Entity;
using ApiWebServer.Common.Define;
using ApiWebServer.Database.Utils;
using ApiWebServer.PBTables;
using WebSharedLib.Contents.Api;
using ApiWebServer.Models;
using ApiWebServer.Core.Controller;
using System.Text;

namespace ApiWebServer.Controllers.CareerModeControllers
{
    [Route("api/CareerMode/[controller]")]
    [ApiController]
    public class CareerModeSpecialTrainingSetController : SessionContoller<ReqCareerModeSpecialTrainingSet, ResCareerModeSpecialTrainingSet>
    {
        public CareerModeSpecialTrainingSetController(
            ILogger<CareerModeSpecialTrainingSetController> logger,
            IConfiguration config, 
            IWebService<ReqCareerModeSpecialTrainingSet, ResCareerModeSpecialTrainingSet> webService, 
            IDBService dbService )
            : base( logger, config, webService, dbService )
        {
        }

        [HttpPost]
        [ApiExplorerSettings( GroupName = "client" )]
        [SwaggerExtend( "커리어모드 특별훈련 셋팅", typeof(CareerModeSpecialTrainingSetPacket) )]
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

            // 그룹 훈련일 경우 선수 정보 미리 저장
            foreach ( var info in reqData.TrainingInfo )
            {
                trainingPlayerList.AddRange( info.PlayerSerials );
            }
            trainingPlayerStr = Common.ServerUtils.MakeSplittedString( trainingPlayerList );

            if ( trainingPlayerList.Count == 0)
            {
                return _webService.End(ErrorCode.ERROR_INVALID_PARAM);
            }

            DataSet dataSet = gameDB.USP_GS_GM_CAREERMODE_SPECIALTRAINING_SET_R(webSession.TokenInfo.Pcid, reqData.Step, trainingPlayerStr);
            if (dataSet == null)
            {
                return _webService.End(ErrorCode.ERROR_DB, "USP_GS_GM_CAREERMODE_SPECIALTRAINING_SET_R");
            }

            DataSetWrapper dataSetWrapper = new DataSetWrapper(dataSet);
            byte career_no = dataSetWrapper.GetValue<byte>(0, "career_no");
            byte mode_level = dataSetWrapper.GetValue<byte>(0, "mode_level");
            byte specialtraining_step = dataSetWrapper.GetValue<byte>(0, "specialtraining_step");
            List<CareerModeSpecialTraining> specialTrainingSetList = dataSetWrapper.GetObjectList<CareerModeSpecialTraining>( 1 );
            List<Player> players = dataSetWrapper.GetObjectList<Player>(2);

            // 유효성 체크
            if ( reqData.CareerNo != career_no ||
                reqData.Step != specialtraining_step )
            {
                return _webService.End( ErrorCode.ERROR_NOT_MATCHING_INFO );
            }
            else if ( mode_level == 0 )
            {
                return _webService.End( ErrorCode.ERROR_NOT_FOUND_MODE_LEVEL );
            }

            //해당 훈련 타입의 전체 리스트
            List<CareerModeSpecialTraining> resultSecialTrainingInfo = new List<CareerModeSpecialTraining>();
            List<AccountPlayerTrainingInfo> trainingPlayerInfo = new List<AccountPlayerTrainingInfo>();

            //중복을 막기위한 체크(요청한 선수수와 db에 있는 중복제거된 선수 수가 일치하지 않으면 에러)
            if ( trainingPlayerList.Count != players.Count)
            {
                return _webService.End(ErrorCode.ERROR_NOT_MATCHING_INFO);
            }

            foreach (CareerModeTrainingInfo info in reqData.TrainingInfo)
            {
                //같은 훈련을 이미 했는지 체크
                if (specialTrainingSetList.Find(x => x.training_type == info.training_id) != null)
                {
                    return _webService.End(ErrorCode.ERROR_ALREADY_SPECIALTRAINING_TRAINING);
                }

                List<Player> playerInfo = players.FindAll(x => info.PlayerSerials.Contains(x.account_player_idx));

                //요청한 선수와 DB에 있는 선수가 맞지 않다면 에러
                if (info.PlayerSerials.Count != playerInfo.Count)
                {
                    return _webService.End(ErrorCode.ERROR_INVALID_PARAM, "specialtraining set player count not match");
                }

                ErrorCode errCode = CacheManager.PBTable.CareerModeTable.SpecialTrainingSet(reqData.Step, info.training_id, playerInfo, ref resultSecialTrainingInfo, ref trainingPlayerInfo);

                if (errCode != ErrorCode.SUCCESS)
                {
                    return _webService.End(errCode, "specialtraining set player count not match");
                }
            }

            if ( gameDB.USP_GS_GM_CAREERMODE_SPECIALTRAINING_SET( webSession.TokenInfo.Pcid, reqData.Step, resultSecialTrainingInfo, trainingPlayerInfo, reqData.IsTrainingFinish) == false)
            {
                return _webService.End( ErrorCode.ERROR_DB, "USP_GS_GM_CAREERMODE_SPECIALTRAINING_SET");
            }

            resData.ResultSpecialTrainingInfo = resultSecialTrainingInfo;

            return _webService.End();
        }
    }
}
