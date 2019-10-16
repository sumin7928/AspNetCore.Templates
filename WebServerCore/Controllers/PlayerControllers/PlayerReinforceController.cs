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
    public class PlayerReinforceController : SessionContoller<ReqPlayerReinforce, ResPlayerReinforce>
    {
        public PlayerReinforceController(
            ILogger<PlayerReinforceController> logger,
            IConfiguration config, 
            IWebService<ReqPlayerReinforce, ResPlayerReinforce> webService, 
            IDBService dbService )
            : base( logger, config, webService, dbService )
        {
        }

        [HttpPost]
        [ApiExplorerSettings( GroupName = "client" )]
        [SwaggerExtend( "선수 강화 및 한계돌파", typeof(PlayerReinforcePacket) )] 
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

            if(reqData.LimitUpFlag == true )
            {
                if(reqData.TryReinforceGrade <= PlayerDefine.PlayerReinforceMax)
                {
                    return _webService.End(ErrorCode.ERROR_INVALID_REINFORCE_LEVEL);
                }
                //한계돌파인데 재료가 없으면 에러
                else if (reqData.MaterialPlayerIdx <= 0)
                {
                    return _webService.End(ErrorCode.ERROR_INVALID_PARAM);
                }
            }
            else
            {
                if(reqData.TryReinforceGrade > PlayerDefine.PlayerReinforceMax)
                {
                    return _webService.End(ErrorCode.ERROR_INVALID_REINFORCE_LEVEL);
                }

                reqData.MaterialPlayerIdx = 0;
            }

            // 포지션 변경하려는 슬롯이 열려 있는 슬롯인지, 껴져있는 선수가 있는지, 설정된 보직값과 비교하여 pb테이블 조건에 맞는지 체크.
            DataSet dataSet = gameDB.USP_GS_GM_PLAYER_REINFORCE_R(webSession.TokenInfo.Pcid, reqData.AccountPlayerIdx, reqData.MaterialPlayerIdx);
            if (dataSet == null)
            {
                return _webService.End(ErrorCode.ERROR_DB, "USP_GS_GM_PLAYER_REINFORCE_R");
            }

            DataSetWrapper dataSetWrapper = new DataSetWrapper(dataSet);

            AccountGame accountGameInfo = dataSetWrapper.GetObject<AccountGame>(0);
            List<Player> players = dataSetWrapper.GetObjectList<Player>(1);
            Player targetPlayer;
            Player materialPlayer;
            //int materialPlayerReinforceGrade = 0;

            if(players.Count == 0)
            {
                return _webService.End(ErrorCode.ERROR_NOT_PLAYER);
            }


            if(reqData.LimitUpFlag == true)
            {
                targetPlayer = players.Find(x=>x.account_player_idx == reqData.AccountPlayerIdx);
                materialPlayer = players.Find(x => x.account_player_idx == reqData.MaterialPlayerIdx);

                if (targetPlayer == null || materialPlayer == null)
                {
                    return _webService.End(ErrorCode.ERROR_NOT_PLAYER);
                }
                else if(targetPlayer.player_idx != materialPlayer.player_idx)
                {
                    return _webService.End(ErrorCode.ERROR_NOT_MATCHING_PLAYER_IDX);
                }
                else if(materialPlayer.is_starting == 1)
                {
                    return _webService.End(ErrorCode.ERROR_NOT_USE_LINEUP_PLAYER);
                }
                else if(materialPlayer.is_lock == true)
                {
                    return _webService.End(ErrorCode.ERROR_ALREADY_LOCKED);
                }


                //materialPlayerReinforceGrade = materialPlayer.reinforce_grade;
                accountGameInfo.now_player -= 1;
            }
            else
            {
                targetPlayer = players.Find(x => x.account_player_idx == reqData.AccountPlayerIdx);

                if (targetPlayer == null)
                {
                    return _webService.End(ErrorCode.ERROR_NOT_PLAYER);
                }
            }

            if(targetPlayer.reinforce_grade != reqData.TryReinforceGrade -1)
            {
                return _webService.End(ErrorCode.ERROR_INVALID_PLAYER_INFO);
            }

            ErrorCode resultError = CacheManager.PBTable.PlayerTable.PlayerReinforceTry(targetPlayer, out bool isSuccess, out byte openSlotIdx, out List <GameRewardInfo> ConsumeList);
            if (resultError != ErrorCode.SUCCESS)
            {
                return _webService.End(resultError);
            }

            ConsumeReward consumeProcess = new ConsumeReward(webSession.TokenInfo.Pcid, gameDB, CONSUME_REWARD_TYPE.CONSUME, false);
            consumeProcess.AddConsume(ConsumeList);
            ErrorCode consumeResult = consumeProcess.Run(ref accountGameInfo, true);
            if (consumeResult != ErrorCode.SUCCESS)
            {
                return _webService.End(consumeResult);
            }

            if (gameDB.USP_GS_GM_PLAYER_REINFORCE(webSession.TokenInfo.Pcid, reqData.AccountPlayerIdx, targetPlayer.reinforce_grade, targetPlayer.reinforce_add_rate, openSlotIdx, reqData.MaterialPlayerIdx, accountGameInfo) == false)
            {
                return _webService.End(ErrorCode.ERROR_DB, "USP_GS_GM_PLAYER_REINFORCE");
            }

            resData.IsSuccess = isSuccess;
            resData.ResultReinforceGrade = targetPlayer.reinforce_grade;
            resData.ResultAddRate = targetPlayer.reinforce_add_rate;
            resData.NowHavePlayerCount = accountGameInfo.now_player;
            resData.ResultAccountCurrency = accountGameInfo;
            resData.NewOpenSlotIdx = openSlotIdx;
            return _webService.End();
        }
    }
}
