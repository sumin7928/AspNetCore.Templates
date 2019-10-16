using Enyim.Caching;
using System.Collections.Generic;
using ApiWebServer.PBTables;
using WebSharedLib.Error;

namespace ApiWebServer.Cache.PBTables
{
    public class MessageTable : ICommonPBTable
    {
        private static readonly ILog _logger = LogManager.GetLogger( typeof( MessageTable ) );

        public Dictionary<int,string> _errors = new Dictionary<int, string>();

        public bool LoadTable( MaguPBTableContext context )
        {
            // todo : 테이블 생기면 코드도 추가해야 함

            _errors.Add( 1, "예상하지 못한 문제가 발생하였습니다." );
            _errors.Add( 2, "게임과의 연결이 끊겼습니다." );
            _errors.Add( 3, "다른 기기에서 접속하였습니다." );
            _errors.Add( 4, "네트워크 접속이 원할하지 않습니다." );
            _errors.Add( 5, "잘못된 계정입니다. 확인 후 다시 시도하여 주세요." );
            _errors.Add( 6, "계정 처리 에러입니다." );

            return true;
        }

        public string GetMessage( int errorCode )
        {
            if( _errors.Count <= 0 )
            {
                return "not found message";
            }

            if ( errorCode < (int)ErrorCode.ERROR_CRITICAL_RANGE )
            {
                return _errors[ 1 ];
            }
            else if ( ( int )ErrorCode.ACCOUNT_ERROR < errorCode && errorCode < (int)ErrorCode.ACCOUNT_ERROR_RANGE )
            {
                return _errors[ 6 ];
            }
            else
            {
                return _errors[ 1 ];
            }
        }
    }
}
