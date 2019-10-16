using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Enyim.Caching;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using StackExchange.Redis.Extensions.Core;
using ApiWebServer.Common;
using ApiWebServer.Core.Helper;
using ApiWebServer.Core.Session;
using ApiWebServer.Logic;
using WebSharedLib;
using WebSharedLib.Contents;
using WebSharedLib.Core.NPLib;
using WebSharedLib.Error;

namespace ApiWebServer.Core.Middleware
{
    public class ChatMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ChatMiddleware> _logger;
        private readonly IConfiguration _config;
        private readonly ICacheClient _cacheClient;

        private ConcurrentDictionary<long, WebSocket> _connSockets = new ConcurrentDictionary<long, WebSocket>();
        private ConcurrentDictionary<long, List<long>> _clanLists = new ConcurrentDictionary<long, List<long>>();

        private long _incrementNo = 0;

        public ChatMiddleware(RequestDelegate next,
            IConfiguration config,
            ILogger<ChatMiddleware> logger,
            ICacheClient cacheClient)
        {
            _next = next;
            _config = config;
            _logger = logger;
            _cacheClient = cacheClient;

            _cacheClient.Subscribe<BroadcastMessage>(ChatDefine.GLOBAL_CHANNEL, message =>
            {
                _connSockets.Values.AsParallel().ForAll(async webSocket =>
                {
                    await webSocket.SendAsync(ChatService.SerializePacket(ChatPacketType.BROADCAST_MESSAGE, message), WebSocketMessageType.Binary, true, CancellationToken.None);
                });
            });

            _cacheClient.Subscribe<BroadcastClanMessage>(ChatDefine.CLAN_CHANNEL, message =>
            {
                if (_clanLists.TryGetValue(message.ClanNo, out List<long> clanUsers) == true)
                {
                    clanUsers.AsParallel().ForAll(async connNo =>
                    {
                        if (_connSockets.TryGetValue(connNo, out WebSocket webSocket) == true)
                        {
                            await webSocket.SendAsync(ChatService.SerializePacket(ChatPacketType.BROADCAST_CLAN_MESSAGE, message), WebSocketMessageType.Binary, true, CancellationToken.None);
                        }
                    });
                }
            });
        }

        public async Task Invoke(HttpContext context)
        {
            if (!context.WebSockets.IsWebSocketRequest)
                return;

            WebSocket webSocket = await context.WebSockets.AcceptWebSocketAsync();
            if (Connected(webSocket, out long requestNo) == false)
            {
                _logger.LogError("[{0}] Failed Connection", requestNo);
            }

            ChatService chatService = new ChatService(requestNo, _cacheClient);

            try
            {
                while (webSocket.State == WebSocketState.Open)
                {
                    byte[] buffer = new byte[1024];
                    WebSocketReceiveResult result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

                    if (result.MessageType == WebSocketMessageType.Close)
                    {
                        _logger.LogInformation("[{0}] Remote socket closed - {1}:{2}", requestNo, result.CloseStatus, result.CloseStatusDescription);
                        await Disconnected(chatService);
                        return;
                    }
                    if (result.MessageType != WebSocketMessageType.Binary)
                    {
                        _logger.LogError("[{0}] Invalidate WebSocketMessageType - {1}", requestNo, result.MessageType);
                        await Disconnected(chatService);
                        return;
                    }

                    byte[] resBuffer = chatService.Process(buffer, result.Count, out bool isLogin);
                    if (isLogin == true)
                    {
                        long clanNo = chatService.ChatSession.ClanNo;
                        if (clanNo > 0)
                        {
                            if (_clanLists.ContainsKey(clanNo) == false)
                            {
                                _clanLists.TryAdd(clanNo, new List<long>());
                            }

                            _clanLists[clanNo].Add(requestNo);
                        }
                    }

                    await webSocket.SendAsync(resBuffer, WebSocketMessageType.Binary, true, CancellationToken.None);
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, "[{0}] WebSocket Middleware Exception : {1}", requestNo, e.Message);
                await Disconnected(chatService);
                return;
            }
        }

        private bool Connected(WebSocket webSocket, out long requestNo)
        {
            requestNo = Interlocked.Increment(ref _incrementNo);
            if (_connSockets.TryAdd(requestNo, webSocket) == false)
            {
                _logger.LogWarning("[{0}] Failed to add websocket", requestNo);
                return false;
            }

            if (webSocket.State != WebSocketState.Open)
            {
                return false;
            }

            return true;
        }

        private async Task Disconnected(ChatService chatService)
        {
            _logger.LogInformation("[{0}] Disconnected", chatService.RequestNo);
            _connSockets.Remove(chatService.RequestNo, out WebSocket removedSocket);
            await removedSocket.CloseAsync(closeStatus: WebSocketCloseStatus.NormalClosure,
                                    statusDescription: "Closed by ChatMiddleware",
                                    cancellationToken: CancellationToken.None);

            if (chatService.ChatSession.ClanNo > 0)
            {
                _clanLists[chatService.ChatSession.ClanNo].Remove(chatService.RequestNo);
                if (_clanLists[chatService.ChatSession.ClanNo].Count == 0)
                {
                    _clanLists.Remove(chatService.ChatSession.ClanNo, out List<long> removedList);
                    if (removedList.Count > 0)
                    {
                        _logger.LogWarning("[{0}] Critial Error - removed clan user");
                    }
                }
            }
        }
    }
}
