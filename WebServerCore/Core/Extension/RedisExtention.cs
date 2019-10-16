using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis.Extensions.Core;
using StackExchange.Redis.Extensions.Core.Configuration;
using StackExchange.Redis.Extensions.MsgPack;

namespace ApiWebServer.Core.Extension
{
    public static class RedisExtention
    {
        private static readonly NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

        public static IServiceCollection AddRedis(this IServiceCollection services, IConfiguration configuration)
        {
            // Register redis
            RedisConfiguration redisConfiguration = configuration.GetSection("Redis").Get<RedisConfiguration>();
            if (redisConfiguration.Connection.IsConnected)
            {
                Console.WriteLine("StackExchangeRedisCacheClient connected redis servers");
            }
            redisConfiguration.Connection.ConnectionRestored += (sender, args) =>
            {
                _logger.Info("Redis connected - {0}, {1}", sender.ToString(), args.EndPoint);
            };
            redisConfiguration.Connection.ConnectionFailed += (sender, args) =>
            {
                _logger.Error(args.Exception, "Failed redis connection - {0}, {1}", sender.ToString(), args.Exception.Message);
            };
            redisConfiguration.Connection.ErrorMessage += (sender, args) =>
            {
                _logger.Error("Redis error Message - {0}, {1}", sender.ToString(), args.Message);
            };
            redisConfiguration.Connection.InternalError += (sender, args) =>
            {
                _logger.Error(args.Exception, "Redis internal error Message - {0}, {1}", sender.ToString(), args.Origin);
            };
            MsgPackObjectSerializer serializer = new MsgPackObjectSerializer();
            StackExchangeRedisCacheClient redisCacheClient = new StackExchangeRedisCacheClient(serializer, redisConfiguration);
            services.AddSingleton<ICacheClient>(redisCacheClient);

            return services;
        }

        //public static IServiceCollection AddRankServer(this IServiceCollection services)
        //{
        //    ICacheClient cacheClient = services.BuildServiceProvider().GetService<ICacheClient>();

        //    return services;
        //}
    }
}
