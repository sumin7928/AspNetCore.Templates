using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using ApiWebServer.Core.Helper;
using ApiWebServer.Models;
using WebSharedLib.Core.NPLib;
using WebSharedLib.Entity;
using WebSharedLib.Error;

namespace ApiWebServer.Core
{
    [Serializable]
    public class WebSession
    {
        private static NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

        [Serializable]
        public class WebTokenInfo
        {
            public string SessionKey { get; set; }
            public long Pcid { get; set; }
            public string ConnTime { get; set; }
            public bool IsCachedSession { get; set; }
        }

        public WebTokenInfo TokenInfo { get; set; }
        public string Token { get; set; }

        public string PubId { get; set; }
        public byte PubType { get; set; }
        public string EncryptedProfileUrl { get; set; }
        public byte OSType { get; set; }
        public int StoreType { get; set; }
        public long Version { get; set; }
        public int TeamIdx { get; set; }
        public byte NationType { get; set; }
        public string UserName { get; set; }
        public byte Sequence { get; set; }
        public byte DBNo { get; set; }
        public string PreviousUrl { get; set; }
        public string BattleKey { get; set; }
        public long ClanNo { get; set; }
        public List<RepeatMission> MissionList { get; set; }
        public List<Achievement> AchievementList { get; set; }

        public override string ToString()
        {
            return $"pubType:{PubType}, pubId:{PubId}, pcId:{TokenInfo.Pcid}";
        }

        public static WebSession CreateFromToken( string token )
        {
            string decrypted = NPCrypt.Decrypt( token, AppConfig.ServerCryptKey, AppConfig.ServerCryptIV );
            if (string.IsNullOrEmpty( decrypted ))
            {
                return default( WebSession );
            }

            WebTokenInfo sessionInfo = JsonConvert.DeserializeObject<WebTokenInfo>( decrypted );

            return WebSessionHelper.GetWebSession( sessionInfo );
        }

        public ErrorCode ValidateSequence( byte requestSequence )
        {
            if ( Sequence != requestSequence )
            {
                if ( Sequence - 1 == requestSequence )
                {
                    return ErrorCode.ERROR_LAST_PACKET_RETRY;
                }

                return ErrorCode.ERROR_SEQUENCE;
            }

            Sequence++;
            return ErrorCode.SUCCESS;
        }

        public ErrorCode UpdateSession()
        {
            // 세션 정보 저장 
            if ( WebSessionHelper.UpdateSession( this ) == false )
            {
                _logger.Error( "Failed to update session info - Pcid:{0}, SessionId:{1}", TokenInfo.Pcid, TokenInfo.SessionKey );
                return ErrorCode.ERROR_SESSION;
            }

            return ErrorCode.SUCCESS;
        }

        public T GetLastPacket<T>()
        {
            return WebSessionHelper.GetLastPacket<T>( this );
        }

        public void SetLastPacket<T>( T resData )
        {
            // 마지막 패킷 저장
            if ( WebSessionHelper.SetLastPacket( this, resData ) == false )
            {
                _logger.Warn( "Failed to update session info - Pcid:{0}, SessionId:{1}", TokenInfo.Pcid, TokenInfo.SessionKey );
            }
        }

        public ErrorCode RegisterSessionLock()
        {
            if ( WebSessionHelper.SetSessionIDLock( TokenInfo ) == false )
            {
                _logger.Warn( "Failed to set session lock - Pcid:{0}, SessionId:{1}", TokenInfo.Pcid, TokenInfo.SessionKey );
                return ErrorCode.ERROR_SESSION_LOCK;
            }

            return ErrorCode.SUCCESS;
        }

        public ErrorCode RemoveSessionLock()
        {
            if ( WebSessionHelper.RemoveSessionIDLock( TokenInfo ) == false )
            {
                _logger.Warn( "Failed to release session lock - Pcid:{0}, SessionId:{1}", TokenInfo.Pcid, TokenInfo.SessionKey );
                return ErrorCode.ERROR_SESSION_LOCK;
            }

            return ErrorCode.SUCCESS;
        }
    }
}
