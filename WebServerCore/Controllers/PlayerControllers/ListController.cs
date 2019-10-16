using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Data;
using ApiWebServer.Core;
using ApiWebServer.Core.Controller;
using ApiWebServer.Core.Swagger;
using ApiWebServer.Database.Utils;
using WebSharedLib.Contents;
using WebSharedLib.Contents.Api;
using WebSharedLib.Core.NPLib;
using WebSharedLib.Entity;
using WebSharedLib.Error;
using ApiWebServer.Models;

namespace ApiWebServer.Controllers.PlayerControllers
{
    [Route("api/Player/[controller]")]
    [ApiController]
    public class ListController : SessionContoller<ReqPlayerList, ResPlayerList>
    {
        public ListController(
            ILogger<ListController> logger,
            IConfiguration config,
            IWebService<ReqPlayerList, ResPlayerList> webService, 
            IDBService dbService )
            : base( logger, config, webService, dbService )
        {
        }

        [HttpPost]
        [ApiExplorerSettings( GroupName = "client" )]
        [SwaggerExtend( "선수 리스트 요청", typeof( PlayerListPacket ) )]
        public NPWebResponse Controller([FromBody] NPWebRequest requestBody )
        {
            WrapWebService( requestBody );
            if ( _webService.ErrorCode != ErrorCode.SUCCESS )
            {
                return _webService.End( _webService.ErrorCode );
            }

            // Business
            var webSession = _webService.WebSession;
            var resData = _webService.WebPacket.ResData;
            var gameDB = _dbService.CreateGameDB(_webService.RequestNo, webSession.DBNo);

            // 캐릭터 리스트( 장착 장비 포함 ) 조회
            DataSet dataSet = gameDB.USP_GS_GM_PLAYERLIST_R(webSession.TokenInfo.Pcid);
            if (dataSet == null)
            {
                return _webService.End( ErrorCode.ERROR_DB, "USP_GS_GM_PLAYERLIST_R" );
            }

            DataSetWrapper dataSetWrapper = new DataSetWrapper( dataSet );            
			List<Player> playerList = dataSetWrapper.GetObjectList<Player>( 0 );

            AccountGame accountGameInfo = dataSetWrapper.GetObject<AccountGame>(1);
            List<Coach> coachList = dataSetWrapper.GetObjectList<Coach>( 2 );

            if ( playerList.Count == 0)
            {
                return _webService.End( ErrorCode.ERROR_DB_ROW_COUNT, "playerList" );
            }

            if (coachList.Count == 0)
            {
                return _webService.End(ErrorCode.ERROR_DB_ROW_COUNT, "coachList" );
            }


            if(accountGameInfo.now_player != playerList.Count || accountGameInfo.now_coach != coachList.Count)
            {
                if (gameDB.USP_GS_GM_PLAYERLIST(webSession.TokenInfo.Pcid, playerList.Count, coachList.Count) == false)
                {
                    return _webService.End(ErrorCode.ERROR_DB, "USP_GS_GM_PLAYERLIST");
                }

                //accountGameInfo.now_player = playerList.Count;
                //accountGameInfo.now_coach = coachList.Count;
            }


            AccountTrainingResult trainingSelectInfo = dataSetWrapper.GetObject<AccountTrainingResult>(3);

            resData.Player = playerList;
            resData.Coach = coachList;
            resData.teamIdx = accountGameInfo.team_idx;
            resData.TrainingSelectInfo = trainingSelectInfo;
            resData.PlayerInvenMaxCount = accountGameInfo.max_player;
            resData.CoachInvenMaxCount = accountGameInfo.max_coach;

            return _webService.End();
        }
    }
}
