using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using ApiWebServer.Cache;
using ApiWebServer.Common.Define;
using ApiWebServer.Core;
using ApiWebServer.Core.Controller;
using ApiWebServer.Core.Swagger;
using ApiWebServer.Database.Utils;
using WebSharedLib.Contents;
using WebSharedLib.Contents.Api;
using WebSharedLib.Core.NPLib;
using WebSharedLib.Entity;
using WebSharedLib.Error;

namespace ApiWebServer.Controllers.PlayerControllers
{
    [Route("api/Player/[controller]")]
    [ApiController]
    public class PlayerLineupRecommendController : SessionContoller<ReqPlayerLineupRecommend, ResPlayerLineupRecommend>
    {
        public PlayerLineupRecommendController(
            ILogger<PlayerLineupRecommendController> logger,
            IConfiguration config, 
            IWebService<ReqPlayerLineupRecommend, ResPlayerLineupRecommend> webService, 
            IDBService dbService )
            : base( logger, config, webService, dbService )
        {
        }

        [HttpPost]
        [ApiExplorerSettings( GroupName = "client" )]
        [SwaggerExtend( "선수 추천 라인업", typeof(PlayerLineupRecommendPacket) )]
        public NPWebResponse Controller( [FromBody] NPWebRequest requestBody )
        {
            WrapWebService( requestBody );
            if ( _webService.ErrorCode != ErrorCode.SUCCESS)
            {
                return _webService.End( _webService.ErrorCode );
            }

            // Business
            var webSession = _webService.WebSession;
            var reqData = _webService.WebPacket.ReqData;
            var resData = _webService.WebPacket.ResData;
            var gameDB = _dbService.CreateGameDB(_webService.RequestNo, webSession.DBNo);

            StringBuilder playerStr = new StringBuilder();
            foreach (PlayerLineupInfo player in reqData.PlayerLineupList)
            {
                playerStr.AppendFormat("{0},", player.account_player_idx);
            }
            playerStr.Remove(playerStr.Length - 1, 1);


            DataSet dataSet = gameDB.USP_GS_GM_PLAYER_LINEUP_RECOMMEND_R(webSession.TokenInfo.Pcid, reqData.ModeType, reqData.PlayerType, playerStr.ToString());
            if (dataSet == null)
            {
                return _webService.End(ErrorCode.ERROR_DB, "USP_GS_GM_PLAYER_LINEUP_RECOMMEND_R");
            }

            DataSetWrapper dataSetWrapper = new DataSetWrapper(dataSet);
            List<CareerModePlayer> myPlayers  = dataSetWrapper.GetObjectList<CareerModePlayer>(0);

            if(reqData.ModeType == (byte)GAME_MODETYPE.MODE_CAREERMODE && myPlayers.FindIndex(x => x.injury_cure_no != 0) != -1)
            {
                return _webService.End(ErrorCode.ERROR_NOT_LINEUP_INJURY_CURE_PLAYER);
            }

            ErrorCode eCode = CacheManager.PBTable.PlayerTable.CheckPlayerLineupValid(reqData.PlayerLineupList, myPlayers.ToDictionary(x => x.account_player_idx, x => x), reqData.PlayerType);
                
            if(eCode != ErrorCode.SUCCESS) 
            {
                return _webService.End(eCode);
            }

            if (gameDB.USP_GS_GM_PLAYER_LINEUP_RECOMMEND(webSession.TokenInfo.Pcid, reqData.ModeType, reqData.PlayerType, JsonConvert.SerializeObject(reqData.PlayerLineupList), (byte)PLAYER_POSITION.INVEN,
                                                        reqData.PlayerType == (byte)PLAYER_TYPE.TYPE_BATTER ? (byte)PLAYER_ORDER.INVEN_BATTER : (byte)PLAYER_ORDER.INVEN_PITCHER) == false)
            {
                _logger.LogError("USP_GS_GM_PLAYER_LINEUP_RECOMMEND - Not exist dataSet. pcID:{0}", webSession.TokenInfo.Pcid);
                return _webService.End(ErrorCode.ERROR_DB);
            }

            return _webService.End();
        }
    }
}
