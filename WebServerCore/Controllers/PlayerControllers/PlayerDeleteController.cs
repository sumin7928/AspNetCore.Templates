using System.Collections.Generic;
using System.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
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
    public class PlayerDeleteController : SessionContoller<ReqPlayerDelete, ResPlayerDelete>
    {
        public PlayerDeleteController(
            ILogger<PlayerDeleteController> logger,
            IConfiguration config, 
            IWebService<ReqPlayerDelete, ResPlayerDelete> webService, 
            IDBService dbService )
            : base( logger, config, webService, dbService )
        {
        }

        [HttpPost]
        [ApiExplorerSettings( GroupName = "client" )]
        [SwaggerExtend( "선수 삭제 처리", typeof(PlayerDeletePacket) )]
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

            byte priceType = 0;
            int priceValue = 0;

            DataSet dataSet = gameDB.USP_GS_GM_PLAYER_DELETE_R(webSession.TokenInfo.Pcid, reqData.AccountPlayerIdxList, reqData.PlayerType);
            if (dataSet == null)
            {
                return _webService.End(ErrorCode.ERROR_DB, "USP_GS_GM_PLAYER_DELETE_R");
            }

            DataSetWrapper dataSetWrapper = new DataSetWrapper(dataSet);
            AccountGame accountGameInfo = dataSetWrapper.GetObject<AccountGame>(0);
            List<PlayerLockDeleteCheck> playerDelete = dataSetWrapper.GetObjectList<PlayerLockDeleteCheck>(1);

            if (reqData.AccountPlayerIdxList.Count != playerDelete.Count)
            {
                return _webService.End(ErrorCode.ERROR_NOT_PLAYER);
            }

            ErrorCode playerDeleteResult;
            playerDeleteResult = Cache.CacheManager.PBTable.PlayerTable.PlayerDeleteCheck(ref playerDelete, reqData.PlayerType, priceType, priceValue, out List<GameRewardInfo> consumeList, ref accountGameInfo, out int nowPlayerCnt);

            if (playerDeleteResult != ErrorCode.SUCCESS)
            {
                return _webService.End(playerDeleteResult);
            }

            ConsumeReward consumeProcess = new ConsumeReward(webSession.TokenInfo.Pcid, gameDB, CONSUME_REWARD_TYPE.REWARD, false);
            consumeProcess.AddReward(consumeList);
            ErrorCode consumeResult = consumeProcess.Run(ref accountGameInfo, true);

            if (consumeResult != ErrorCode.SUCCESS)
            {
                return _webService.End(consumeResult);
            }

            if (false == gameDB.USP_GS_GM_PLAYER_DELETE(webSession.TokenInfo.Pcid, reqData.AccountPlayerIdxList, reqData.PlayerType, accountGameInfo))
            {
                return _webService.End(ErrorCode.ERROR_DB, "USP_GS_GM_PLAYER_DELETE");
            }

            resData.NowHavePlayerCount = nowPlayerCnt;
            resData.ResultAccountCurrency = accountGameInfo;

            return _webService.End();
        }
    }
}
