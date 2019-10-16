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
    public class ItemDeleteController : SessionContoller<ReqItemDelete, ResItemDelete>
    {
        public ItemDeleteController(
            ILogger<ItemDeleteController> logger,
            IConfiguration config, 
            IWebService<ReqItemDelete, ResItemDelete> webService, 
            IDBService dbService )
            : base( logger, config, webService, dbService )
        {
        }

        [HttpPost]
        [ApiExplorerSettings( GroupName = "client" )]
        [SwaggerExtend( "아이템 삭제", typeof(ItemDeletePacket) )]
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

            // 유저 정보 가져옴
            DataSet gameDataSet = gameDB.USP_GS_GM_ACCOUNT_ITEM_DELETE_R(webSession.TokenInfo.Pcid, reqData.ItemIdx);
            if (gameDataSet == null)
            {
                return _webService.End(ErrorCode.ERROR_DB, "USP_GS_GM_ACCOUNT_ITEM_DELETE_R");
            }

            DataSetWrapper gameDataSetWrapper = new DataSetWrapper(gameDataSet);

            if (gameDataSetWrapper.GetRowCount(0) == 0)
            {
                return _webService.End(ErrorCode.ERROR_NO_ACCOUNT);
            }

            if (gameDataSetWrapper.GetRowCount(1) == 0)
            {
                return _webService.End(ErrorCode.ERROR_NOT_HAVE_ITEM);
            }

            // 보상 처리
            if (gameDB.USP_GS_GM_ACCOUNT_ITEM_DELETE(webSession.TokenInfo.Pcid, reqData.ItemIdx) == false)
            {
                return _webService.End(ErrorCode.ERROR_DB, "USP_GS_GM_ACCOUNT_ITEM_DELETE");
            }

            return _webService.End();
        }
    }
}
