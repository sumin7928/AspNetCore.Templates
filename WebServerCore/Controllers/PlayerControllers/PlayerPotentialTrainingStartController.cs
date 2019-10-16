using System.Collections.Generic;
using System.Data;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using ApiWebServer.Cache;
using ApiWebServer.Common.Define;
using ApiWebServer.Core;
using ApiWebServer.Core.Controller;
using ApiWebServer.Core.Swagger;
using ApiWebServer.Database.Utils;
using ApiWebServer.Logic;
using ApiWebServer.Models;
using WebSharedLib.Contents;
using WebSharedLib.Contents.Api;
using WebSharedLib.Core.NPLib;
using WebSharedLib.Entity;
using WebSharedLib.Error;

namespace ApiWebServer.Controllers.PlayerControllers
{
    [Route("api/Player/[controller]")]
    [ApiController]
    public class PlayerPotentialTrainingStartController : SessionContoller<ReqPlayerPotentialTrainingStart, ResPlayerPotentialTrainingStart>
    {
        public PlayerPotentialTrainingStartController(
            ILogger<PlayerPotentialTrainingStartController> logger,
            IConfiguration config, 
            IWebService<ReqPlayerPotentialTrainingStart, ResPlayerPotentialTrainingStart> webService, 
            IDBService dbService )
            : base( logger, config, webService, dbService )
        {
        }

        [HttpPost]
        [ApiExplorerSettings( GroupName = "client" )]
        [SwaggerExtend( "선수 잠재력 훈련 시작", typeof(PlayerPotentialTrainingStartPacket) )] 
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

            if(reqData.MaterialPlayerIdx.Count != PlayerDefine.PlayerPotentialMaterialCount)
            {
                return _webService.End(ErrorCode.ERROR_NOT_MATCHING_MATERIAL_COUNT);
            }

            string materialPlayerData = Common.ServerUtils.MakeSplittedString(reqData.MaterialPlayerIdx);
            string allPlayerData = materialPlayerData + "," + reqData.AccountPlayerIdx;
            DataSet dataSet = gameDB.USP_GS_GM_PLAYER_POTENTIAL_TRAINING_START_R(webSession.TokenInfo.Pcid, allPlayerData);
            if (dataSet == null)
            {
                return _webService.End(ErrorCode.ERROR_DB, "USP_GS_GM_PLAYER_POTENTIAL_TRAINING_START_R");
            }

            DataSetWrapper dataSetWrapper = new DataSetWrapper(dataSet);

            AccountGame accountGameInfo = dataSetWrapper.GetObject<AccountGame>(0);
            List<Player> allPlayerList = dataSetWrapper.GetObjectList<Player>(1);
            bool isAlreadyFlag = dataSetWrapper.GetValue<bool>(2, "use_flag");

            Player targetPlayer = allPlayerList.Find(x=>x.account_player_idx == reqData.AccountPlayerIdx);
            List<Player> materialPlayers = allPlayerList.FindAll(x => x.account_player_idx != reqData.AccountPlayerIdx);

            if (accountGameInfo == null)
            {
                return _webService.End(ErrorCode.ERROR_NO_ACCOUNT);
            }

            //이미 진행하고 있는 훈련이 있다면 에러
            //임시처리 ( 나중에 꼭 주석풀어야함)
            /*if (isAlreadyFlag == true)
            {
                return _webService.End(ErrorCode.ERROR_REQUEST_DATA);
            }*/

            if (targetPlayer == null || materialPlayers == null || materialPlayers.Count != reqData.MaterialPlayerIdx.Count)
            {
                return _webService.End(ErrorCode.ERROR_NOT_PLAYER);
            }

            //열린 잠재력이 1개라도 있는지 체크
            byte possibleSlotCount = CacheManager.PBTable.PlayerTable.PlayerPotentialPossibleSlotCount(targetPlayer.reinforce_grade);
            if (possibleSlotCount <= 0)
            {
                return _webService.End(ErrorCode.ERROR_INVALID_PARAM);
            }

            //잠재력 개발한게 1개라도 있는지 체크
            if (targetPlayer.potential_idx1 <= 0 && targetPlayer.potential_idx2 <= 0 && targetPlayer.potential_idx3 <= 0)
            {
                return _webService.End(ErrorCode.ERROR_NOT_CREATE_POTENTIAL_INFO);
            }

            // 재료로 사옹되는 선수 체크
            ErrorCode errorCode = CacheManager.PBTable.PlayerTable.CheckMaterialPlayers(materialPlayers, out int addRate);
            if (errorCode != ErrorCode.SUCCESS)
            {
                return _webService.End(errorCode);
            }

            // 잠재력 활성
            CacheManager.PBTable.PlayerTable.PlayerPotentialTraining(targetPlayer, possibleSlotCount, addRate);

            GameRewardInfo consumeList = new GameRewardInfo((byte)CacheManager.PBTable.ConstantTable.Const.potential_rankup_cost_type, 0, CacheManager.PBTable.ConstantTable.Const.potential_rankup_cost_count);
            ConsumeReward consumeProcess = new ConsumeReward(webSession.TokenInfo.Pcid, gameDB, CONSUME_REWARD_TYPE.CONSUME, false);
            consumeProcess.AddConsume(consumeList);
            ErrorCode consumeResult = consumeProcess.Run(ref accountGameInfo, true);
            if (consumeResult != ErrorCode.SUCCESS)
            {
                return _webService.End(consumeResult);
            }

            accountGameInfo.now_player -= materialPlayers.Count;


            if (gameDB.USP_GS_GM_PLAYER_POTENTIAL_TRAINING_START(webSession.TokenInfo.Pcid, targetPlayer, materialPlayerData, accountGameInfo) == false)
            {
                return _webService.End(ErrorCode.ERROR_DB, "USP_GS_GM_PLAYER_POTENTIAL_TRAINING_START");
            }

            resData.PlayerPotentialIdx1 = targetPlayer.potential_idx1;
            resData.PlayerPotentialIdx2 = targetPlayer.potential_idx2;
            resData.PlayerPotentialIdx3 = targetPlayer.potential_idx3;
            resData.NowHavePlayerCount = accountGameInfo.now_player;
            resData.ResultAccountCurrency = accountGameInfo;
            
            return _webService.End();
        }
    }
}
