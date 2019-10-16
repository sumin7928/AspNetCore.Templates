using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Data;
using ApiWebServer.Core;
using ApiWebServer.Core.Controller;
using ApiWebServer.Core.Swagger;
using ApiWebServer.Database.Utils;
using ApiWebServer.Models;
using WebSharedLib;
using WebSharedLib.Contents;
using WebSharedLib.Contents.Api;
using WebSharedLib.Core.NPLib;
using WebSharedLib.Error;
namespace ApiWebServer.Controllers.AccountControllers
{
    [Route( "api/Account/[controller]" )]
    [ApiController]
    public class BindAccountController : SessionContoller<ReqBindAccount, ResBindAccount>
    {
        public BindAccountController(
            ILogger<BindAccountController> logger,
            IConfiguration config,
            IWebService<ReqBindAccount, ResBindAccount> webService,
            IDBService dbService )
            : base( logger, config, webService, dbService )
        {
        }

        [HttpPost]
        [ApiExplorerSettings( GroupName = "client" )]
        [SwaggerExtend( "계정 연동", typeof( BindAccountPacket ) )]
        public NPWebResponse Controller( [FromBody] NPWebRequest requestBody )
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
            var accountDB = _dbService.CreateAccountDB( _webService.RequestNo );

            // pubType 검증
            if ( reqData.PubType > PubType.GUEST && reqData.PubType < PubType.MAX )
            {
                //이미 연동한 퍼블리셔로 또 연동하려고 하면 에러
                if ( webSession.PubType == reqData.PubType )
                {
                    return _webService.End( ErrorCode.ERROR_PUBLISHER_TYPE, $"now pubType:{webSession.PubType}, request pubType:{reqData.PubType}" );
                }
            }
            else
            {
                return _webService.End( ErrorCode.ERROR_PUBLISHER_TYPE, $"pubType:{reqData.PubType}" );
            }

            // 계정 조회
            DataSet dataSet = accountDB.USP_AC_BIND_ACCOUNT_R(webSession.TokenInfo.Pcid);
            if (dataSet == null)
            {
                return _webService.End( ErrorCode.ERROR_NO_ACCOUNT, "USP_AC_BIND_ACCOUNT_R", $"pubType:{reqData.PubType}" );
            }

            DataSetWrapper dataSetWrapper = new DataSetWrapper( dataSet );
            List<AccountPublisher> publisherList = dataSetWrapper.GetObjectList<AccountPublisher>( 0 );

            foreach ( var item in publisherList )
            {
                //해당 타입이 이미 있을경우
                if ( item.pub_type == reqData.PubType )
                {
                    if ( item.pub_id == reqData.PubID )
                    {
                        return _webService.End( ErrorCode.ERROR_BIND_ALREADY_ACCOUNT, $"pubType:{reqData.PubType}" );
                    }
                    else
                    {
                        return _webService.End( ErrorCode.ERROR_BIND_ANOTHER_PUBID, $"pubType:{reqData.PubType}" );
                    }
                }
            }

            // 계정 연결
            if ( accountDB.USP_AC_BIND_ACCOUNT( webSession.TokenInfo.Pcid, reqData.PubType, reqData.PubID ) == false )
            {
                return _webService.End( ErrorCode.ERROR_BIND_ACCOUNT, "USP_AC_BIND_ACCOUNT", $"pubType:{reqData.PubType}" );
            }

            webSession.PubType = reqData.PubType;
            webSession.PubId = reqData.PubID;

            return _webService.End();
        }
    }
}
