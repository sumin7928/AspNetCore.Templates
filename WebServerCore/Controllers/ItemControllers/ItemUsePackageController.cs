using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
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

namespace ApiWebServer.Controllers.ItemControllers
{
    [Route("api/Item/[controller]")]
    [ApiController]
    public class ItemUsePackageController : SessionContoller<ReqItemUsePackage, ResItemUsePackage>
    {
        public ItemUsePackageController(
            ILogger<ItemUsePackageController> logger,
            IConfiguration config, 
            IWebService<ReqItemUsePackage, ResItemUsePackage> webService, 
            IDBService dbService )
            : base( logger, config, webService, dbService )
        {
        }

        [HttpPost]
        [ApiExplorerSettings( GroupName = "client" )]
        [SwaggerExtend( "패키지 아이템 사용", typeof(ItemUsePackagePacket) )]
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

            List<GameRewardInfo> ItemList = CacheManager.PBTable.ItemTable.GetPackegeItem(reqData.ItemIdx );
            if(ItemList == null)
            {
                return _webService.End( ErrorCode.ERROR_REQUEST_DATA);
            }

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

            // 보상 정보 처리
            ConsumeReward consumeReward = new ConsumeReward(webSession.TokenInfo.Pcid, gameDB, CONSUME_REWARD_TYPE.CONSUMEREWARD, false);
            consumeReward.AddConsume(new GameRewardInfo((byte)REWARD_TYPE.NORMAL_ITEM, reqData.ItemIdx, 1));
            consumeReward.AddReward(ItemList);

            ErrorCode rewardResult = consumeReward.Run(ref accountGameInfo, false);
            if (rewardResult != ErrorCode.SUCCESS)
            {
                return _webService.End(rewardResult);
            }

            // 보상 처리
            if (gameDB.USP_GS_GM_REWARD_PROCESS(webSession.TokenInfo.Pcid, accountGameInfo, consumeReward.GetUpdateItemList()) == false)
            {
                return _webService.End(ErrorCode.ERROR_DB, "USP_GS_GM_REWARD_PROCESS");
            }

            resData.ResultAccountCurrency = accountGameInfo;
            resData.RewardInfo = ItemList;
            resData.UpdateItemInfo = consumeReward.GetUpdateItemList();

            return _webService.End();
        }
    }
}
