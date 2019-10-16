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
using ApiWebServer.Logic;
using ApiWebServer.Common;

namespace ApiWebServer.Controllers.CareerModeControllers
{
    [Route("api/CareerMode/[controller]")]
    [ApiController]
    public class CareerModeInjuryCureEndController : SessionContoller<ReqCareerModeInjuryCureEnd, ResCareerModeInjuryCureEnd>
    {
        public CareerModeInjuryCureEndController(
            ILogger<CareerModeInjuryCureEndController> logger,
            IConfiguration config, 
            IWebService<ReqCareerModeInjuryCureEnd, ResCareerModeInjuryCureEnd> webService, 
            IDBService dbService )
            : base( logger, config, webService, dbService )
        {
        }

        [HttpPost]
        [ApiExplorerSettings( GroupName = "client" )]
        [SwaggerExtend( "커리어모드 부상선수 치료 완료", typeof(CareerModeInjuryCureEndPacket) )]
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
            
            string playerData = ServerUtils.MakeSplittedString( reqData.CureAccountPlayerList );

            DataSet dataSet = gameDB.USP_GS_GM_CAREERMODE_INJURY_CURE_END_R(webSession.TokenInfo.Pcid, playerData);
            if (dataSet == null)
            {
                return _webService.End(ErrorCode.ERROR_DB, "USP_GS_GM_CAREERMODE_INJURY_CURE_END_R");
            }

            DataSetWrapper dataSetWrapper = new DataSetWrapper(dataSet);
            CareerModeInfo careerModeInfo = dataSetWrapper.GetObject<CareerModeInfo>(0);
            List<CareerModePlayer> players = dataSetWrapper.GetObjectList<CareerModePlayer>(1);
            AccountGame accountGameInfo = dataSetWrapper.GetObject<AccountGame>(2);

            // 유효성 체크
            if ( reqData.CareerNo != careerModeInfo.career_no)
            {
                return _webService.End( ErrorCode.ERROR_NOT_MATCHING_INFO );
            }
            else if ( players.Count != reqData.CureAccountPlayerList.Count )
            {
                return _webService.End( ErrorCode.ERROR_NOT_PLAYER);
            }

            List<PlayerCareerInjuryInfo> updatePlayerInjuryInfo = new List<PlayerCareerInjuryInfo>();
            int calcCureValue = 0;
            bool isDirectCure = false;

            foreach (CareerModePlayer playerInfo in players)
            {
                if(playerInfo.injury_idx == 0)
                {
                    return _webService.End(ErrorCode.ERROR_NOT_INJURY_PLAYER);
                }
                else if(playerInfo.is_starting == 1)
                {
                    return _webService.End(ErrorCode.ERROR_NOT_INJURY_CURE_LINEUP_PLAYER);
                }

                //즉시완료라면
                if (reqData.CureCostValue > 0)
                {
                    //아직부상 치료안했다면
                    if (playerInfo.injury_cure_no == 0)
                    {
                        // 다이아 += 비용 * 상처기간
                        calcCureValue += CacheManager.PBTable.CareerModeTable.ManagementConfig.InstantHeal_Cost * playerInfo.injury_period;
                    }
                    else //치료중인 선수라면
                    {
                        //이미 치료가 완료된 선수가 있음
                        if (playerInfo.injury_cure_no <= careerModeInfo.game_no)
                        {
                            return _webService.End(ErrorCode.ERROR_FINISH_INJURY_CURE_PLAYER);
                        }

                        // 다이아 += 비용 * ( 남은 게임차수 ) 
                        calcCureValue += CacheManager.PBTable.CareerModeTable.ManagementConfig.InstantHeal_Cost * (playerInfo.injury_cure_no - careerModeInfo.game_no);
                    }
                }
                else //일반 완료라면
                {
                    
                    if(playerInfo.injury_cure_no == 0)
                    {
                        //치료시작안했으면 에러
                        return _webService.End(ErrorCode.ERROR_NOT_INJURY_CURE_START_PLAYER);
                    }
                    else if (playerInfo.injury_cure_no > careerModeInfo.game_no)
                    {
                        //아직 치료가 안끝났으면 에러
                        return _webService.End(ErrorCode.ERROR_NOT_ENOUGH_INJURY_CURE_PERIOD);
                    }
                }

                ErrorCode resultCure = CacheManager.PBTable.CareerModeTable.DoCureInjury( playerInfo.injury_idx, careerModeInfo );
                if( resultCure != ErrorCode.SUCCESS )
                {
                    return _webService.End(ErrorCode.ERROR_STATIC_DATA);
                }

                updatePlayerInjuryInfo.Add(new PlayerCareerInjuryInfo() {
                    account_player_idx = playerInfo.account_player_idx,
                    injury_idx = 0,
                    injury_period = 0,
                    injury_add_ratio = 0,
                    injury_cure_no = 0
                });
            }

            //즉시완료라면 유효성 체크 및 재화차감
            if (reqData.CureCostValue > 0)
            {
                isDirectCure = true;

                //서버에서 계산한 금액과 클라에서 계산한 금액이 맞지않음
                if (calcCureValue != reqData.CureCostValue)
                {
                    return _webService.End(ErrorCode.ERROR_INVALID_PARAM);
                }

                ConsumeReward consumeProcess = new ConsumeReward(webSession.TokenInfo.Pcid, gameDB, CONSUME_REWARD_TYPE.CONSUME, false);
                consumeProcess.AddConsume(new GameRewardInfo((byte)CacheManager.PBTable.CareerModeTable.ManagementConfig.InstantHeal_CostType, 0, calcCureValue));
                ErrorCode consumeResult = consumeProcess.Run(ref accountGameInfo, true);
                if (consumeResult != ErrorCode.SUCCESS)
                {
                    return _webService.End(consumeResult);
                }
            }

            if ( gameDB.USP_GS_GM_CAREERMODE_INJURY_CURE_END( webSession.TokenInfo.Pcid, careerModeInfo, playerData, accountGameInfo, isDirectCure) == false)
            {
                return _webService.End( ErrorCode.ERROR_DB, "USP_GS_GM_CAREERMODE_INJURY_CURE_END");
            }

            resData.UpdatePlayerInjuryInfo = updatePlayerInjuryInfo;
            resData.ResultAccountCurrency = accountGameInfo;

            return _webService.End();
        }
    }
}
