using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading.Tasks;
using Enyim.Caching;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using StackExchange.Redis.Extensions.Core;
using ApiWebServer.Common;
using ApiWebServer.Core.Session;
using WebSharedLib;
using WebSharedLib.Contents;
using WebSharedLib.Core.NPLib;
using WebSharedLib.Error;

namespace ApiWebServer.Core
{
    public class ChatService
    {
        private static NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

        public long RequestNo { get; private set; }
        public ICacheClient CacheClient { get; private set; }

        public ChatSession ChatSession { get; private set; } = new ChatSession();

        public ChatService(long requestNo, ICacheClient cacheClient)
        {
            RequestNo = requestNo;
            CacheClient = cacheClient;
        }

        public byte[] Process(byte[] reqBuffer, int reqLength, out bool isLogin)
        {
            isLogin = false;

            // buffer 분기 처리
            NPChatRequestHeader reqHeader = reqBuffer.MarshalDeserializer<NPChatRequestHeader>(0, ChatDefine.HEADER_SIZE);

            if (reqHeader.PacketNo != ++ChatSession.PacketNo)
            {
                return SerializePacket(reqHeader.Type, ErrorCode.ERROR_CHAT_PACKET_SEQ);
            }

            string reqData = Encoding.UTF8.GetString(reqBuffer, ChatDefine.HEADER_SIZE, reqLength - ChatDefine.HEADER_SIZE);

            switch (reqHeader.Type)
            {
                // 로그인 패킷 처리
                case ChatPacketType.LOGIN:
                {
                    ReqChatLogin loginData = JsonConvert.DeserializeObject<ReqChatLogin>(reqData);
                    if (loginData.Token == null || loginData.Token == string.Empty)
                    {
                        return SerializePacket(reqHeader.Type, ErrorCode.ERROR_CHAT_PACKET_INVALID_DATA);
                    }

                    // 토큰으로 세션 로그인 확인 처리
                    WebSession session = WebSession.CreateFromToken(loginData.Token);
                    if (session == null)
                    {
                        return SerializePacket(reqHeader.Type, ErrorCode.ERROR_CHAT_NOT_FOUND_SESSION);
                    }

                    long loginTime = ServerUtils.GetNowLocalTimeStemp();

                    ChatSession.Pcid = session.TokenInfo.Pcid;
                    ChatSession.UserName = session.UserName;
                    ChatSession.LoginTime = loginTime;
                    ChatSession.ClanNo = session.ClanNo;

                    ResChatLogin resChatLogin = new ResChatLogin
                    {
                        LoginTime = loginTime
                    };

                    isLogin = true;
                    return SerializePacket(reqHeader.Type, resChatLogin);
                }
                // 메세지 패킷 처리
                case ChatPacketType.MESSAGE:
                {
                    // 로그인 체크
                    if (ChatSession.Pcid == 0)
                    {
                        return SerializePacket(reqHeader.Type, ErrorCode.ERROR_CHAT_NOT_FOUND_SESSION);
                    }

                    ReqChatMessage reqChatMessage = JsonConvert.DeserializeObject<ReqChatMessage>(reqData);
                    if (reqChatMessage.MessageType == ChatMessageType.GLOBAL_CHAT)
                    {
                        BroadcastMessage message = new BroadcastMessage
                        {
                            MessageType = reqChatMessage.MessageType,
                            Nickname = ChatSession.UserName,
                            Message = reqChatMessage.Message
                        };

                        CacheClient.Publish(ChatDefine.GLOBAL_CHANNEL, message);
                    }
                    else if (reqChatMessage.MessageType == ChatMessageType.CLAN_CHAT)
                    {
                        if (ChatSession.ClanNo <= 0)
                        {
                            return SerializePacket(reqHeader.Type, ErrorCode.ERROR_CHAT_NOT_FOUND_CLAN_INFO);
                        }

                        BroadcastClanMessage message = new BroadcastClanMessage
                        {
                            ClanNo = ChatSession.ClanNo,
                            MessageType = reqChatMessage.MessageType,
                            Nickname = ChatSession.UserName,
                            Message = reqChatMessage.Message
                        };

                        CacheClient.Publish(ChatDefine.CLAN_CHANNEL, message);
                    }
                    else
                    {
                        return SerializePacket(reqHeader.Type, ErrorCode.ERROR_CHAT_PACKET_NOT_FOUND_TYPE);
                    }

                    ResChatMessage resChatMessage = new ResChatMessage();
                    return SerializePacket(reqHeader.Type, resChatMessage);
                }
                default:
                {
                    return SerializePacket(reqHeader.Type, ErrorCode.ERROR_CHAT_PACKET_NOT_FOUND_TYPE);
                }
            }
        }

        public static byte[] SerializePacket(int type, object reqeustData = null)
        {
            return SerializePacket(type, ErrorCode.SUCCESS, reqeustData);
        }

        public static byte[] SerializePacket(int type, ErrorCode errorCode, object requestData = null)
        {
            string jsonData = requestData != null ? JsonConvert.SerializeObject(requestData) : string.Empty;
            NPChatResponseHeader header = new NPChatResponseHeader
            {
                Type = type,
                TotalLength = jsonData.Length + ChatDefine.HEADER_SIZE,
                ErrorCode = (int)errorCode
            };

            byte[] packetBytes = new byte[header.TotalLength];

            byte[] headerBytes = header.MarshalSerializer();
            Array.Copy(headerBytes, 0, packetBytes, 0, headerBytes.Length);

            if (jsonData != null && jsonData.Length > 0)
            {
                byte[] dataBytes = Encoding.UTF8.GetBytes(jsonData);
                Array.Copy(dataBytes, 0, packetBytes, headerBytes.Length, dataBytes.Length);
            }

            return packetBytes;
        }

    }
}
