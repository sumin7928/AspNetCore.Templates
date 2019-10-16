using Enyim.Caching;
using Enyim.Caching.Memcached;
using Enyim.Caching.Memcached.Results;
using Microsoft.Extensions.Configuration;
using MsgPack.Serialization;
using Newtonsoft.Json;
using System;
using System.Data;
using System.Threading;
using ApiWebServer.Common;
using ApiWebServer.Common.Define;
using ApiWebServer.Database;
using ApiWebServer.Database.Utils;
using WebSharedLib.Core.NPLib;
using static ApiWebServer.Core.WebSession;

namespace ApiWebServer.Core.Helper
{
    public class WebSessionHelper
    {
        private static readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        public static readonly string sessionLock = "lock";
        public static readonly string lastPacket = "lastPacket";

        public static int incrementNo;

        public static IConfiguration _configuration;
        public static IDBService _dbService;
        public static MemcachedClient _memcachedClient;

        public static bool Initialize( IConfiguration configuration, IMemcachedClient memcachedClient, IDBService dbService )
        {
            _configuration = configuration;
            _memcachedClient = ( MemcachedClient )memcachedClient;
            //_memcachedClient.NodeFailed
            _dbService = dbService;

            // session config check
            if( _configuration[ "SessionTimeout" ] == null || 
                _configuration[ "LastPacketTimeout" ] == null || 
                _configuration[ "LockTimeout" ] == null )
            {
                return false;
            }

            return true;
        }

        public static string CreateSessionKey()
        {
            return KeyGenerator.Instance.GetIncrementKey(GAME_KEY_TYPE.SESSION_KEY);
        }

        public static WebSession GetWebSession( WebTokenInfo tokenInfo )
        {
            string key = tokenInfo.Pcid.ToString();

            if ( tokenInfo.IsCachedSession )
            {
                IGetOperationResult result = _memcachedClient.ExecuteGet( key );
                if ( result.Success == false )
                {
                    logger.Debug( "Failed to get session from memcached - pcId:{0}, key:{1}, message:{2}", tokenInfo.Pcid, key, result.Message );
                    return default( WebSession );
                }
                else
                {
                    return MessagePackSerializer.Get<WebSession>().UnpackSingleObject((byte[])result.Value);
                }
            }
            else
            {
                // DB 처리
                AccountDB accountDB = _dbService.CreateAccountDB( 0 );
                DataSet dataSet = accountDB.USP_AC_SESSION_R(tokenInfo.Pcid);
                if (dataSet == null)
                {
                    logger.Warn( "Failed to get session from DB - pcId:{0}", tokenInfo.Pcid );
                    return default( WebSession );
                }

                DataSetWrapper dataSetWrapper = new DataSetWrapper( dataSet );
                string sessionData = dataSetWrapper.GetValue<string>( 0, "session_data" );

                return JsonConvert.DeserializeObject<WebSession>( sessionData );
            }
        }

        public static bool InputSession( WebSession session )
        {
            if ( session == null )
            {
                return false;
            }

            string key = session.TokenInfo.Pcid.ToString();

            session.TokenInfo.IsCachedSession = true;
            session.Token = NPCrypt.Encrypt( JsonConvert.SerializeObject( session.TokenInfo ), AppConfig.ServerCryptKey, AppConfig.ServerCryptIV );

            DateTime expiredTime = DateTime.UtcNow.AddSeconds( int.Parse( _configuration[ "SessionTimeout" ] ) );
            ArraySegment<byte> sessionData = MessagePackSerializer.Get<WebSession>().PackSingleObjectAsBytes(session);
            IStoreOperationResult result = _memcachedClient.ExecuteStore( StoreMode.Set, key, sessionData, expiredTime );
            if ( result.Success == false )
            {
                logger.Warn( "Failed to input session in memcached - pcId:{0}, key:{1}, value:{2}", session.TokenInfo.Pcid, key, session );

                // DB 처리
                session.TokenInfo.IsCachedSession = false;
                session.Token = NPCrypt.Encrypt( JsonConvert.SerializeObject( session.TokenInfo ), AppConfig.ServerCryptKey, AppConfig.ServerCryptIV );

                AccountDB accountDB = _dbService.CreateAccountDB( 0 );
                string jsonSession = JsonConvert.SerializeObject( session );
                if ( accountDB.USP_AC_SESSION( session.TokenInfo.Pcid, jsonSession ) == false )
                {
                    logger.Error( "Failed to input session in DB - pcId:{0}, session:{1}", session.TokenInfo.Pcid, jsonSession );
                    return false;
                }
            }

            return true;
        }

        public static bool UpdateSession( WebSession session )
        {
            if ( session == null )
            {
                return false;
            }

            string key = session.TokenInfo.Pcid.ToString();

            if ( session.TokenInfo.IsCachedSession )
            {
                DateTime expiredTime = DateTime.UtcNow.AddSeconds( int.Parse( _configuration[ "SessionTimeout" ] ) );
                ArraySegment<byte> sessionData = MessagePackSerializer.Get<WebSession>().PackSingleObjectAsBytes(session);
                IStoreOperationResult result = _memcachedClient.ExecuteStore( StoreMode.Set, key, sessionData, expiredTime );
                var stats = _memcachedClient.Stats();
                if ( result.Success == false )
                {
                    logger.Error( "Failed to update session in memcached - pcId:{0}, key:{1}, value:{2}", session.TokenInfo.Pcid, key, session );
                    return false;
                }
            }
            else
            {
                // DB 처리
                AccountDB accountDB = _dbService.CreateAccountDB( 0 );
                string jsonSession = JsonConvert.SerializeObject( session );
                if ( accountDB.USP_AC_SESSION( session.TokenInfo.Pcid, jsonSession ) == false )
                {
                    logger.Error( "Failed to update session in DB - pcId:{0}, sesison:{1}", session.TokenInfo.Pcid, jsonSession );
                    return false;
                }
            }

            return true;
        }


        public static bool SetLastPacket<T>( WebSession session, T resData )
        {
            string key = session.TokenInfo.Pcid.ToString() + lastPacket;

            if ( session.TokenInfo.IsCachedSession )
            {
                TimeSpan expiredTime = new TimeSpan( 0, 0, int.Parse( _configuration[ "LastPacketTimeout" ] ) );
                var result = _memcachedClient.ExecuteStore( StoreMode.Set, key, resData, expiredTime );
                if ( result.Success == false )
                {
                    logger.Error( "Failed to set last packet in memcached - pcId:{0}, key:{1}, value:{2}", session.TokenInfo.Pcid, key, resData );
                    return false;
                }
            }
            else
            {
                // DB 처리
                AccountDB accountDB = _dbService.CreateAccountDB( 0 );
                string lastPacketString = JsonConvert.SerializeObject( resData );
                if ( accountDB.USP_AC_SESSION_LAST_PACKET( session.TokenInfo.Pcid, lastPacketString ) == false )
                {
                    logger.Error( "Failed to set last packet in gameDB - pcId:{0}, packet:{1}", session.TokenInfo.Pcid, lastPacketString );
                    return false;
                }
            }

            return true;
        }

        public static T GetLastPacket<T>( WebSession session )
        {
            string key = session.TokenInfo.Pcid.ToString() + lastPacket;

            if ( session.TokenInfo.IsCachedSession )
            {
                var result = _memcachedClient.ExecuteGet( key );
                if ( result.Success == false )
                {
                    logger.Error( "Failed to get last packet from memcached - pcId:{0}, key:{1}, message:{2}", session.TokenInfo.Pcid, key, result.Message );
                    return default( T );
                }

                return JsonConvert.DeserializeObject<T>( result.Value.ToString() );
            }
            else
            {
                // DB 처리
                AccountDB accountDB = _dbService.CreateAccountDB( 0 );
                DataSet dataSet = accountDB.USP_AC_SESSION_LAST_PACKET_R(session.TokenInfo.Pcid);
                if (dataSet == null)
                {
                    logger.Error( "Failed to get last packet from DB - pcId:{0}", session.TokenInfo.Pcid );
                    return default( T );
                }

                DataSetWrapper dataSetWrapper = new DataSetWrapper( dataSet );
                string lastPacket = dataSetWrapper.GetValue<string>( 0, "last_packet" );

                return JsonConvert.DeserializeObject<T>( lastPacket );
            }
        }

        public static bool SetSessionIDLock( WebTokenInfo webToken )
        {
            string key = webToken.SessionKey + sessionLock;

            if ( webToken.IsCachedSession )
            {
                TimeSpan expiredTime = new TimeSpan( 0, 0, int.Parse( _configuration[ "LockTimeout" ] ) );
                var result = _memcachedClient.ExecuteStore( StoreMode.Add, key, "", expiredTime );
                if ( result.StatusCode == 2 ) // 이미 있는데 호출할 경우
                {
                    logger.Warn( "Session was locked - pcId:{0}, sessionId:{1}", webToken.Pcid, key );
                    return false;
                }
            }
            else
            {
                // DB 처리
                AccountDB accountDB = _dbService.CreateAccountDB( 0 );
                if ( accountDB.USP_AC_SESSION_ID_LOCK( webToken.SessionKey, (byte)DB_SESSION_LOCK.SET_LOCK, int.Parse( _configuration[ "LockTimeout" ] ) ) == false )
                {
                    logger.Warn( "Session was locked - pcId:{0}, sessionId:{1}", webToken.Pcid, key );
                    return false;
                }
            }

            return true;
        }

        public static bool RemoveSessionIDLock( WebTokenInfo webToken )
        {
            string key = webToken.SessionKey + sessionLock;

            if ( webToken.IsCachedSession )
            {
                var result = _memcachedClient.ExecuteRemove( key );
                if ( result.InnerResult != null && result.InnerResult.StatusCode == 1 ) // 없을 경우
                {
                    logger.Error( "Failed to remove session lock in memcached - pcId:{0}, sessionId:{1}", webToken.Pcid, key );
                    return true;
                }
            }
            else
            {
                // DB 처리
                AccountDB accountDB = _dbService.CreateAccountDB( 0 );
                if ( accountDB.USP_AC_SESSION_ID_LOCK( webToken.SessionKey, ( byte )DB_SESSION_LOCK.REMOVE_LOCK, int.Parse( _configuration[ "LockTimeout" ] ) ) == false )
                {
                    logger.Error( "Failed to remove session lock in DB - pcId:{0}, sessionId:{1}", webToken.Pcid, key );
                    return false;
                }
            }

            return true;
        }
    }
}
