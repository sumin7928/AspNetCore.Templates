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

namespace ApiWebServer.Controllers.ItemControllers
{
    [Route("api/Item/[controller]")]
    [ApiController]
    public class ItemInfoController : SessionContoller<ReqItemInfo, ResItemInfo>
    {
        public ItemInfoController(
            ILogger<ItemInfoController> logger,
            IConfiguration config, 
            IWebService<ReqItemInfo, ResItemInfo> webService, 
            IDBService dbService )
            : base( logger, config, webService, dbService )
        {
        }

        [HttpPost]
        [ApiExplorerSettings( GroupName = "client" )]
        [SwaggerExtend( "아이템 정보", typeof(ItemInfoPacket) )]
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
            DataSet itemDataSet = gameDB.USP_GS_GM_ACCOUNT_ITEM_R(webSession.TokenInfo.Pcid, "");
            if (itemDataSet == null)
            {
                return _webService.End(ErrorCode.ERROR_DB, "USP_GS_GM_ACCOUNT_ITEM_R");
            }

            DataSetWrapper itemDataSetWrapper = new DataSetWrapper(itemDataSet);
            List<ItemInven> itemInfo = itemDataSetWrapper.GetObjectList<ItemInven>(0);

            resData.ItemInfo = itemInfo;
            return _webService.End();
        }
    }
}
