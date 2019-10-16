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
    public class CareerModeInjuryCureStartController : SessionContoller<ReqCareerModeInjuryCureStart, ResCareerModeInjuryCureStart>
    {
        public CareerModeInjuryCureStartController(
            ILogger<CareerModeInjuryCureStartController> logger,
            IConfiguration config, 
            IWebService<ReqCareerModeInjuryCureStart, ResCareerModeInjuryCureStart> webService, 
            IDBService dbService )
            : base( logger, config, webService, dbService )
        {
        }

        [HttpPost]
        [ApiExplorerSettings( GroupName = "client" )]
        [SwaggerExtend( "커리어모드 부상선수 치료 시작", typeof(CareerModeInjuryCureStartPacket) )]
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

            string playerData = Common.ServerUtils.MakeSplittedString( reqData.CureAccountPlayerList );

            DataSet dataSet = gameDB.USP_GS_GM_CAREERMODE_INJURY_CURE_START_R(webSession.TokenInfo.Pcid, playerData);
            if (dataSet == null)
            {
                return _webService.End(ErrorCode.ERROR_DB, "USP_GS_GM_CAREERMODE_INJURY_CURE_START_R");
            }

            DataSetWrapper dataSetWrapper = new DataSetWrapper(dataSet);
            byte career_no = dataSetWrapper.GetValue<byte>(0, "career_no");
            int game_no = dataSetWrapper.GetValue<int>(0, "game_no");
            List<CareerModePlayer> players = dataSetWrapper.GetObjectList<CareerModePlayer>(1);

            // 유효성 체크
            if ( reqData.CareerNo != career_no  )
            {
                return _webService.End( ErrorCode.ERROR_NOT_MATCHING_INFO );
            }
            else if ( players.Count != reqData.CureAccountPlayerList.Count )
            {
                return _webService.End( ErrorCode.ERROR_NOT_PLAYER);
            }

            List<PlayerCareerInjuryInfo> updatePlayerInjuryInfo = new List<PlayerCareerInjuryInfo>();

            foreach (CareerModePlayer playerInfo in players)
            {
                if(playerInfo.injury_idx == 0)
                {
                    return _webService.End(ErrorCode.ERROR_NOT_INJURY_PLAYER);
                }
                else if(playerInfo.injury_cure_no != 0)
                {
                    return _webService.End(ErrorCode.ERROR_ALREADY_INJURY_CURE_PLAYER);
                }
                else if(playerInfo.is_starting == 1)
                {
                    return _webService.End(ErrorCode.ERROR_NOT_INJURY_CURE_LINEUP_PLAYER);
                }

                updatePlayerInjuryInfo.Add( new PlayerCareerInjuryInfo() {
                    account_player_idx = playerInfo.account_player_idx,
                    injury_idx = playerInfo.injury_idx,
                    injury_period = playerInfo.injury_period,
                    injury_add_ratio = playerInfo.injury_add_ratio,
                    injury_cure_no = game_no + playerInfo.injury_period
                });
            }

            if ( gameDB.USP_GS_GM_CAREERMODE_INJURY_CURE_START( webSession.TokenInfo.Pcid, updatePlayerInjuryInfo) == false)
            {
                return _webService.End( ErrorCode.ERROR_DB, "USP_GS_GM_CAREERMODE_INJURY_CURE_START");
            }

            resData.UpdatePlayerInjuryInfo = updatePlayerInjuryInfo;
            
            return _webService.End();
        }
    }
}
