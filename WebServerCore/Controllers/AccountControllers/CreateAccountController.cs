using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Data;
using ApiWebServer.Core;
using ApiWebServer.Core.Controller;
using ApiWebServer.Core.Swagger;
using ApiWebServer.Database.Utils;
using WebSharedLib;
using WebSharedLib.Contents;
using WebSharedLib.Contents.Api;
using WebSharedLib.Core.NPLib;
using WebSharedLib.Error;
using ApiWebServer.Common.Define;
using ApiWebServer.Common;

namespace ApiWebServer.Controllers.AccountControllers
{
    [Route("api/Account/[controller]")]
    [ApiController]
    public class CreateAccountController : NonSessionController<ReqCreateAccount, ResCreateAccount>
    {

        public CreateAccountController( 
            ILogger<CreateAccountController> logger,
            IConfiguration config, 
            IWebService<ReqCreateAccount, ResCreateAccount> webService, 
            IDBService dbService )
            : base( logger, config, webService, dbService )

        {
        }

        [HttpPost]
        [ApiExplorerSettings( GroupName = "client" )]
        [SwaggerExtend( "계정 생성", typeof( CreateAccountPacket ) )]
        public NPWebResponse Controller( [FromBody] NPWebRequest requestBody )
        {
            WrapWebService( requestBody );
            if ( _webService.ErrorCode != ErrorCode.SUCCESS)
            {
                return _webService.End( _webService.ErrorCode );
            }

            // Business
            var reqData = _webService.WebPacket.ReqData;
            var resData = _webService.WebPacket.ResData;
            var accountDB = _dbService.CreateAccountDB( _webService.RequestNo );

            if (reqData.PubType == PubType.GUEST)
            {
                reqData.PubID = DateTime.Now.ToString("HHmmss") + Guid.NewGuid().ToString("N");
                resData.IsGuest = true;
            }
            else if (reqData.PubType > PubType.GUEST && reqData.PubType < PubType.MAX)
            {
                if (string.IsNullOrEmpty(reqData.PubID) == true)
                {
                    return _webService.End(ErrorCode.ERROR_INVALID_PARAM);
                }

                resData.IsGuest = false;
            }
            else
            {
                return _webService.End(ErrorCode.ERROR_PUBLISHER_TYPE);
            }

            string nickName = GetAutoNickName();

            // 계정 조회
            DataSet dataSet = accountDB.USP_AC_CREATE_ACCOUNT_R(reqData.PubType, reqData.PubID, nickName);
            if ( dataSet == null )
            {
                return _webService.End( ErrorCode.ERROR_DB, "USP_AC_CREATE_ACCOUNT_R", $"pubType:{reqData.PubType}" );
            }

            DataSetWrapper dataSetWrapper = new DataSetWrapper( dataSet );
            long pcId = dataSetWrapper.GetValue<long>( 0, "pc_id" );
            byte dbNum = dataSetWrapper.GetValue<byte>( 0, "db_num" );

            if (resData.IsGuest == true)
                resData.PubID = reqData.PubID = dbNum.ToString() + reqData.PubID;
            else
                resData.PubID = "";

            var gameDB = _dbService.CreateGameDB( _webService.RequestNo, dbNum );

            //임시처리 - 추후 넷마블sdk에서 받아야함
            reqData.NationalCode = "KOREA";
            // 국가 코드 서버내에서 사용하는 타입으로 변환
            byte serviceNationType = GetServiceNationType(reqData.NationalCode);


            int playerMaxCount = Cache.CacheManager.PBTable.PlayerTable.GetInvenMaxCount(CHARACTER_INVEN_TYPE.PLAYER, 1);
            int coachMaxCount = Cache.CacheManager.PBTable.PlayerTable.GetInvenMaxCount(CHARACTER_INVEN_TYPE.COACH, 1);

            if(playerMaxCount < 1 || coachMaxCount < 1)
            {
                return _webService.End(ErrorCode.ERROR_STATIC_DATA);
            }

            using ( accountDB.BeginTransaction() )
            {
                // AccountDB 계정 생성
                if ( accountDB.USP_AC_CREATE_ACCOUNT( pcId, dbNum, reqData.PubType, reqData.PubID, reqData.OSType, reqData.DeviceID, serviceNationType, nickName, reqData.NationalCode) == false )
                {
                    return _webService.End( ErrorCode.ERROR_DB, "USP_AC_CREATE_ACCOUNT", $"pubType:{reqData.PubType}" );
                }

                using ( gameDB.BeginTransaction() )
                {
                    // GameDB 계정 생성
                    if ( gameDB.USP_GS_GM_ACCOUNT_CREATE( pcId, serviceNationType, playerMaxCount, coachMaxCount, nickName, reqData.NationalCode) == false )
                    {
                        return _webService.End( ErrorCode.ERROR_DB, "USP_GS_GM_ACCOUNT_CREATE", $"pubType:{reqData.PubType}" );
                    }
                    gameDB.Commit();
                }
                accountDB.Commit();
            }

            return _webService.End();
        }

        private byte GetServiceNationType(string nationalCode)
        {
            if (nationalCode == "KOREA")
            {
                return (byte)SERVICE_NATION_TYPE.KOREA;
            }
            else if(nationalCode == "JAPAN")
            {
                return (byte)SERVICE_NATION_TYPE.JAPAN;
            }
            else if (nationalCode == "TAIWAN")
            {
                return (byte)SERVICE_NATION_TYPE.TAIWAN;
            }
            else
            {
                return (byte)SERVICE_NATION_TYPE.AMERICA;
            }
        }

        private string GetAutoNickName()
        {
            string tempNick = KeyGenerator.Instance.GetIncrementKey(GAME_KEY_TYPE.TEMP_NICK_NAME);
            return "U" + tempNick;
        }
    }
}
