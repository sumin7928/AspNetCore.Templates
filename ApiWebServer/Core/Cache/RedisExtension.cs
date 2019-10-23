using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis.Extensions.Core.Configuration;
using StackExchange.Redis.Extensions.Newtonsoft;

namespace ApiServer.Core.Cache
{
    public static class RedisExtension
    {
        private static readonly NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

        public static IServiceCollection AddRedis(this IServiceCollection services, IConfiguration configuration)
        {
            // Register redis
            RedisConfiguration redisConfiguration = configuration.GetSection("Redis").Get<RedisConfiguration>();
            if (redisConfiguration == null)
            {
                Console.WriteLine("Not found redis configuration");
                return services;
            }

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

            services.AddStackExchangeRedisExtensions<NewtonsoftSerializer>(redisConfiguration);
            return services;
        }
    }
}
