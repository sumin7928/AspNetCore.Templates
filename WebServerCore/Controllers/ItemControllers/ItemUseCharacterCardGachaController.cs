using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Data;
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
using System.Collections.Generic;

namespace ApiWebServer.Controllers.ItemControllers
{
    [Route("api/Item/[controller]")]
    [ApiController]
    public class ItemUseCharacterCardGachaController : SessionContoller<ReqItemUseCharacterCardGacha, ResItemUseCharacterCardGacha>
    {
        public ItemUseCharacterCardGachaController(
            ILogger<ItemUseCharacterCardGachaController> logger,
            IConfiguration config, 
            IWebService<ReqItemUseCharacterCardGacha, ResItemUseCharacterCardGacha> webService, 
            IDBService dbService )
            : base( logger, config, webService, dbService )
        {
        }

        [HttpPost]
        [ApiExplorerSettings( GroupName = "client" )]
        [SwaggerExtend( "선수카드 아이템 가챠 사용", typeof(ItemUseCharacterCardGachaPacket) )]
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

            // 유저 정보 가져옴
            DataSet gameDataSet = gameDB.USP_GS_GM_ACCOUNT_GAME_ONLY_R(webSession.TokenInfo.Pcid);
            if (gameDataSet == null)
            {
                return _webService.End(ErrorCode.ERROR_DB, "USP_GS_GM_ACCOUNT_GAME_ONLY_R");
            }

            DataSetWrapper gameDataSetWrapper = new DataSetWrapper(gameDataSet);
            AccountGame accountGameInfo = gameDataSetWrapper.GetObject<AccountGame>(0);

            if (CacheManager.PBTable.ItemTable.IsPossibleUseItem(reqData.ItemIdx, webSession.NationType) == false)
            {
                return _webService.End(ErrorCode.ERROR_NOT_USE_ITEM);
            }

            ErrorCode errorCode = CacheManager.PBTable.ItemTable.GetCharacterCardGacha(CHARACTER_OBTAIN_TYPE.OBTAIN_ITEM, reqData.ItemIdx, reqData.UseCount, accountGameInfo,
                out List<Player> obtainPlayer, out List<Coach> obtainCoach);

            if(errorCode != ErrorCode.SUCCESS)
            {
                return _webService.End(errorCode);
            }


            // 보상 정보 처리
            ConsumeReward consumeReward = new ConsumeReward(webSession.TokenInfo.Pcid, gameDB, CONSUME_REWARD_TYPE.CONSUME, false);
            consumeReward.AddConsume(new GameRewardInfo((byte)REWARD_TYPE.NORMAL_ITEM, reqData.ItemIdx, reqData.UseCount));
            ErrorCode rewardResult = consumeReward.Run(ref accountGameInfo, false);
            if (rewardResult != ErrorCode.SUCCESS)
            {
                return _webService.End(rewardResult);
            }

            // 보상 처리
            DataSet characterDataSet = gameDB.USP_GS_GM_REWARD_PROCESS_WITH_CHARACTER(webSession.TokenInfo.Pcid, accountGameInfo, consumeReward.GetUpdateItemList(), obtainPlayer, obtainCoach);
            if (characterDataSet == null)
            {
                return _webService.End(ErrorCode.ERROR_DB, "USP_GS_GM_REWARD_PROCESS_WITH_CHARACTER");
            }

            //선수 / 코치 인덱스 처리

            DataSetWrapper dataSetWrapperCharacter = new DataSetWrapper(characterDataSet);

            if (obtainPlayer.Count > 0)
            {
                string playerAccountIdxs = dataSetWrapperCharacter.GetValue<string>(0, "account_player_idxs");

                string[] strIdxArr = playerAccountIdxs.Split(',');

                for (int i = 0; i < obtainPlayer.Count; ++i)
                {
                    obtainPlayer[i].account_player_idx = long.Parse(strIdxArr[i]);
                }
            }

            if (obtainCoach.Count > 0)
            {
                string coachAccountIdxs = dataSetWrapperCharacter.GetValue<string>(0, "account_coach_idxs");
                string[] strIdxArr = coachAccountIdxs.Split(',');

                for (int i = 0; i < obtainCoach.Count; ++i)
                {
                    obtainCoach[i].account_coach_idx = long.Parse(strIdxArr[i]);
                }
            }

            resData.UpdateItemInfo = consumeReward.GetUpdateItemList();
            resData.ObtainPlayers = obtainPlayer;
            resData.ObtainCoachs = obtainCoach;
            resData.NowHavePlayerCount = accountGameInfo.now_player;
            resData.NowHaveCoachCount = accountGameInfo.now_coach;

            return _webService.End();
        }
    }
}
