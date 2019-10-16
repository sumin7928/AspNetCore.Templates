using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Data;
using System.Threading.Tasks;
using ApiWebServer.Cache;
using ApiWebServer.Common;
using ApiWebServer.Core;
using ApiWebServer.Core.Controller;
using ApiWebServer.Core.Swagger;
using ApiWebServer.Database.Utils;
using WebSharedLib.Contents;
using WebSharedLib.Contents.Api;
using WebSharedLib.Core.NPLib;
using WebSharedLib.Error;

namespace ApiWebServer.Controllers.AccountControllers
{
    //[ApiVersion( "client" )]
    [Route("api/Account/[controller]")]
    [ApiController]
    public class CreateUserNameController : SessionContoller<ReqCreateUserName, ResCreateUserName>
    {

        public CreateUserNameController( 
            ILogger<CreateUserNameController> logger,
            IConfiguration config,
            IWebService<ReqCreateUserName, ResCreateUserName> webService, 
            IDBService dbService )
            : base( logger, config, webService, dbService )
        {
        }

        [ HttpPost]
        [ApiExplorerSettings( GroupName = "client" )]
        [SwaggerExtend( "유저 닉네임 생성", typeof( CreateUserNamePacket ) )]
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
            var gameDB = _dbService.CreateGameDB( _webService.RequestNo, webSession.DBNo );

            // 금지어 체크
            if ( IsValidNickName( reqData.UserName) == false)
            {
                return _webService.End( ErrorCode.ERROR_FORBIDDEN_WORD, $"nickName:{reqData.UserName}" );
            }

            // AccountDB 계정 정보 호출
            DataSet dataSet = accountDB.USP_AC_CREATE_USER_NAME_R(webSession.TokenInfo.Pcid, reqData.UserName);
            if (dataSet == null)
            {
                return _webService.End( ErrorCode.ERROR_DB, "USP_AC_CREATE_USER_NAME_R", $"nickName:{reqData.UserName}" );
            }

            DataSetWrapper dataSetWrapper = new DataSetWrapper( dataSet );
            bool isFindAccount = dataSetWrapper.GetValue<bool>( 0, "is_find_account" );
            bool isFindName = dataSetWrapper.GetValue<bool>( 0, "is_find_name" );

            // 계정없음 에러
            if (isFindAccount == false)
            {
                return _webService.End(ErrorCode.ERROR_NO_ACCOUNT, $"nickName:{reqData.UserName}" );
            }

            // 중복체크 에러
            if(isFindName == true)
            {
                return _webService.End(ErrorCode.ERROR_OVERLAP_NICKNAME, $"nickName:{reqData.UserName}" );
            }

            using( accountDB.BeginTransaction() )
            {
                // AccountDB 에 닉네임 저장
                if ( accountDB.USP_AC_CREATE_USER_NAME( webSession.TokenInfo.Pcid, reqData.UserName ) == false )
                {
                    return _webService.End( ErrorCode.ERROR_DB, "USP_AC_CREATE_USER_NAME", $"nickName:{reqData.UserName}" );
                }

                using ( gameDB.BeginTransaction() )
                {
                    // GameDB 에 닉네임 저장
                    if ( gameDB.USP_GM_CREATE_USER_NAME( webSession.TokenInfo.Pcid, reqData.UserName ) == false )
                    {
                        return _webService.End( ErrorCode.ERROR_DB, "USP_GM_CREATE_USER_NAME", $"nickName:{reqData.UserName}" );
                    }
                    gameDB.Commit();
                }
                accountDB.Commit();
            }

            webSession.UserName = reqData.UserName;

            resData.UserName = reqData.UserName;
            resData.ExtraCMOX = webSession.Token;

            return _webService.End();

        }

        private bool IsValidNickName( string name )
        {
            if ( name == null || name == string.Empty )
            {
                return false;
            }

            //임시처리
            if (name.Length < 1 || name.Length > 100)
                return false;

            /*
            // 문자열 길이 확인
            if ( name.Length < 2 || name.Length > 12 )
            {
                return false;
            }
            
            // 특수문자 확인
            string str = @"[~!@\#$%^&*\()\=+|\\/:;?""<>']";
            System.Text.RegularExpressions.Regex rex = new System.Text.RegularExpressions.Regex( str );
            if ( rex.IsMatch( name ) )
            {
                return false;
            }

            // 공백 확인
            string str2 = @"\s";
            System.Text.RegularExpressions.Regex rex2 = new System.Text.RegularExpressions.Regex( str2 );
            if ( rex2.IsMatch( name ) )
            {
                return false;
            }

            // 금지어 확인
            if ( CacheManager.PBTable.ForbiddenWordTable.IsForbiddenWord( name ) == true )
            {
                return false;
            }
            */
            return true;
        }
    }
}
