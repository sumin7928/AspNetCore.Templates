using System.Collections.Generic;
using System.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using ApiWebServer.Core;
using ApiWebServer.Core.Controller;
using ApiWebServer.Core.Swagger;
using ApiWebServer.Database.Utils;
using ApiWebServer.Models;
using WebSharedLib.Contents;
using WebSharedLib.Contents.Api;
using WebSharedLib.Core.NPLib;
using WebSharedLib.Error;

namespace ApiWebServer.Controllers.PlayerControllers
{
    [Route("api/Player/[controller]")]
    [ApiController]
    public class LockController : SessionContoller<ReqLock, ResLock>
    {
        public LockController(
            ILogger<LockController> logger,
            IConfiguration config, 
            IWebService<ReqLock, ResLock> webService, 
            IDBService dbService )
            : base( logger, config, webService, dbService )
        {
        }

        [HttpPost]
        [ApiExplorerSettings( GroupName = "client" )]
        [SwaggerExtend( "선수 잠금 처리", typeof( LockPacket ) )]
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

            DataSet dataSet = gameDB.USP_GS_GM_PLAYER_LOCK_R(webSession.TokenInfo.Pcid, reqData.AccountPlayerIdxList, reqData.PlayerType);
            if (dataSet == null)
            {
                return _webService.End(ErrorCode.ERROR_DB, "USP_GS_GM_PLAYER_LOCK_R");
            }

            DataSetWrapper dataSetWrapper = new DataSetWrapper(dataSet);
            List<PlayerLockDeleteCheck> playerLocks = dataSetWrapper.GetObjectList<PlayerLockDeleteCheck>(0);

            if (reqData.AccountPlayerIdxList.Count != playerLocks.Count)
            {
                return _webService.End(ErrorCode.ERROR_NOT_PLAYER);
            }

            ErrorCode playerLockResult;
            playerLockResult = Cache.CacheManager.PBTable.PlayerTable.PlayerLockCheck(ref playerLocks);

            if (playerLockResult != ErrorCode.SUCCESS)
            {
                return _webService.End(playerLockResult);
            }

            if (false == gameDB.USP_GS_GM_PLAYER_LOCK(webSession.TokenInfo.Pcid, playerLocks, reqData.PlayerType))
            {
                return _webService.End(ErrorCode.ERROR_DB, "USP_GS_GM_PLAYER_LOCK");
            }

            return _webService.End();
        }
    }
}
