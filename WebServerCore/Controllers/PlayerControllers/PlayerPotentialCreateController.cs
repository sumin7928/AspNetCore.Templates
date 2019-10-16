using System.Collections.Generic;
using System.Data;
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
    public class PlayerPotentialCreateController : SessionContoller<ReqPlayerPotentialCreate, ResPlayerPotentialCreate>
    {
        public PlayerPotentialCreateController(
            ILogger<PlayerPotentialCreateController> logger,
            IConfiguration config, 
            IWebService<ReqPlayerPotentialCreate, ResPlayerPotentialCreate> webService, 
            IDBService dbService )
            : base( logger, config, webService, dbService )
        {
        }

        [HttpPost]
        [ApiExplorerSettings( GroupName = "client" )]
        [SwaggerExtend( "선수 잠재력 개발 / 재개발", typeof(PlayerPotentialCreatePacket) )] 
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

            // 포지션 변경하려는 슬롯이 열려 있는 슬롯인지, 껴져있는 선수가 있는지, 설정된 보직값과 비교하여 pb테이블 조건에 맞는지 체크.
            DataSet dataSet = gameDB.USP_GS_GM_PLAYER_POTENTIAL_CREATE_R(webSession.TokenInfo.Pcid, reqData.AccountPlayerIdx);
            if (dataSet == null)
            {
                return _webService.End(ErrorCode.ERROR_DB, "USP_GS_GM_PLAYER_POTENTIAL_CREATE_R");
            }

            DataSetWrapper dataSetWrapper = new DataSetWrapper(dataSet);

            AccountGame accountGameInfo = dataSetWrapper.GetObject<AccountGame>(0);
            Player targetPlayer = dataSetWrapper.GetObject<Player>(1);
            byte already_flag = dataSetWrapper.GetValue<byte>(2, "use_flag");

            if (accountGameInfo == null)
            {
                return _webService.End(ErrorCode.ERROR_NO_ACCOUNT);
            }

            if(targetPlayer == null)
            {
                return _webService.End(ErrorCode.ERROR_NOT_PLAYER);
            }

            //이미 진행하고 있는 훈련이 있다면 에러
            //임시처리 ( 나중에 꼭 주석풀어야함)
            /*if (already_flag == 1)
            {
                return _webService.End(ErrorCode.ERROR_REQUEST_DATA);
            }*/

            byte possibleSlotCount = CacheManager.PBTable.PlayerTable.PlayerPotentialPossibleSlotCount(targetPlayer.reinforce_grade);
            List<int> expectBasicIdxList = new List<int>();
            int exceptPotentialIdx = -1;
            int prePotentialIdx = -1;

            if (possibleSlotCount < reqData.SlotIdx)
            {
                return _webService.End(ErrorCode.ERROR_INVALID_PARAM);
            }

            if (reqData.SlotIdx == 1)
            {
                prePotentialIdx = targetPlayer.potential_idx1;

                if (possibleSlotCount > 2)
                    exceptPotentialIdx = CacheManager.PBTable.PlayerTable.SameBasicPotentialCheck(targetPlayer.potential_idx2, targetPlayer.potential_idx3);
            }
            else if (reqData.SlotIdx == 2)
            {
                if (targetPlayer.potential_idx1 <= 0)
                    return _webService.End(ErrorCode.ERROR_NOT_OPEN_PLAYER_POTENTIAL_SLOT);

                prePotentialIdx = targetPlayer.potential_idx2;

                if (possibleSlotCount > 2)
                    exceptPotentialIdx = CacheManager.PBTable.PlayerTable.SameBasicPotentialCheck(targetPlayer.potential_idx1, targetPlayer.potential_idx3);
            }
            else if (reqData.SlotIdx == 3)
            {
                if (targetPlayer.potential_idx1 <= 0 || targetPlayer.potential_idx2 <= 0)
                    return _webService.End(ErrorCode.ERROR_NOT_OPEN_PLAYER_POTENTIAL_SLOT);

                prePotentialIdx = targetPlayer.potential_idx3;

                if (possibleSlotCount > 2)
                    exceptPotentialIdx = CacheManager.PBTable.PlayerTable.SameBasicPotentialCheck(targetPlayer.potential_idx1, targetPlayer.potential_idx2);
            }

            if(exceptPotentialIdx > 0)
            {
                expectBasicIdxList.Add(exceptPotentialIdx);
            }

            List<GameRewardInfo> consumeList = new List<GameRewardInfo>();

            if (reqData.ReCreateFlag == true)
            {
                //재개발 일때 
                if (prePotentialIdx <= 0)
                {
                    return _webService.End(ErrorCode.ERROR_INVALID_PARAM);
                }

                int nowBasicIdx = CacheManager.PBTable.PlayerTable.GetPotentialBasicIdx(prePotentialIdx);
                //중복이 될수가 없음
                if(expectBasicIdxList.FindIndex(x=>x == nowBasicIdx) != -1)
                {
                    return _webService.End(ErrorCode.ERROR_DB_DATA);
                }

                expectBasicIdxList.Add(nowBasicIdx);

                consumeList.Add(new GameRewardInfo((byte)CacheManager.PBTable.ConstantTable.Const.potential_reset_cost_type1, 0, CacheManager.PBTable.ConstantTable.Const.potential_reset_cost_count1));
                consumeList.Add(new GameRewardInfo((byte)CacheManager.PBTable.ConstantTable.Const.potential_reset_cost_type2, 0, CacheManager.PBTable.ConstantTable.Const.potential_reset_cost_count2));
            }
            else
            {
                //개발 일때 
                if (prePotentialIdx > 0)
                {
                    return _webService.End(ErrorCode.ERROR_INVALID_PARAM);
                }

                consumeList.Add(new GameRewardInfo((byte)CacheManager.PBTable.ConstantTable.Const.potential_open_cost_type1, 0, CacheManager.PBTable.ConstantTable.Const.potential_open_cost_count1));
                consumeList.Add(new GameRewardInfo((byte)CacheManager.PBTable.ConstantTable.Const.potential_open_cost_type2, 0, CacheManager.PBTable.ConstantTable.Const.potential_open_cost_count2));
            }

            ConsumeReward consumeProcess = new ConsumeReward(webSession.TokenInfo.Pcid, gameDB, CONSUME_REWARD_TYPE.CONSUME, false);
            consumeProcess.AddConsume(consumeList);
            ErrorCode consumeResult = consumeProcess.Run(ref accountGameInfo, true);
            if (consumeResult != ErrorCode.SUCCESS)
            {
                return _webService.End(consumeResult);
            }

            int potentialIdx = CacheManager.PBTable.PlayerTable.PlayerPotentialCreateIdx(GAME_MODETYPE.MODE_PVP, (PLAYER_TYPE)targetPlayer.player_type, expectBasicIdxList);

            if (gameDB.USP_GS_GM_PLAYER_POTENTIAL_CREATE(webSession.TokenInfo.Pcid, reqData.AccountPlayerIdx, reqData.ReCreateFlag, reqData.SlotIdx, potentialIdx, accountGameInfo) == false)
            {
                return _webService.End(ErrorCode.ERROR_DB, "USP_GS_GM_PLAYER_POTENTIAL_CREATE");
            }

            resData.ResultPotentialIdx = potentialIdx;
            resData.ResultAccountCurrency = accountGameInfo;

            return _webService.End();
        }
    }
}
