using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using StackExchange.Redis.Extensions.Core;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
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

namespace ApiWebServer.Controllers.LiveSeasonControllers
{
    [Route("api/LiveSeason/[controller]")]
    [ApiController]
    public class BattleLineupController : SessionContoller<ReqBattleLineup, ResBattleLineup>
    {
        private readonly RankServer _rankServer;

        public BattleLineupController(
           ILogger<BattleLineupController> logger,
           IConfiguration config,
           IWebService<ReqBattleLineup, ResBattleLineup> webService,
           IDBService dbService,
           ICacheClient redisClient)
           : base(logger, config, webService, dbService)
        {
            _rankServer = new RankServer(redisClient, logger);
        }

        [HttpPost]
        [ApiExplorerSettings(GroupName = "client")]
        [SwaggerExtend("대전 라인업 정보 조회", typeof(BattleLineupPacket))]
        public NPWebResponse Controller([FromBody] NPWebRequest requestBody)
        {
            WrapWebService(requestBody);
            if (_webService.ErrorCode != ErrorCode.SUCCESS)
            {
                return _webService.End(_webService.ErrorCode);
            }

            // Business
            var webSession = _webService.WebSession;
            var reqData = _webService.WebPacket.ReqData;
            var resData = _webService.WebPacket.ResData;
            var gameDB = _dbService.CreateGameDB(_webService.RequestNo, webSession.DBNo);

            long targetUserIdx = webSession.TokenInfo.Pcid;

            if (reqData.UserIdx > 0)
            {
                targetUserIdx = reqData.UserIdx;
            }

            // 정보 가져옴
            DataSet dataSet = gameDB.USP_GM_BATTLE_LINEUP_R(targetUserIdx);
            if (dataSet == null)
            {
                return _webService.End(ErrorCode.ERROR_DB, "USP_GS_GM_LIVESEASON_COMPETITION_DETAIL_INFO_R");
            }

            DataSetWrapper enemyDataSetWrapper = new DataSetWrapper(dataSet);
            List<BattlePlayer> playerList = enemyDataSetWrapper.GetObjectList<BattlePlayer>(0);
            List<BattleCoach> coachList = enemyDataSetWrapper.GetObjectList<BattleCoach>(1);

            if (playerList == null || coachList == null || playerList.Count == 0 )
            {
                return _webService.End(ErrorCode.ERROR_NOT_FOUND_BATTLE_LINEUP_INFO);
            }

            resData.PlayerList = playerList;
            resData.CoachList = coachList;

            return _webService.End();

        }
    }
}
