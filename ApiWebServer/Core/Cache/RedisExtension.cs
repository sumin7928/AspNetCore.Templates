using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis.Extensions.Core.Configuration;
using StackExchange.Redis.Extensions.Newtonsoft;

namespace ApiServer.Core.Cache
{
    public static class RedisExtension
    {
        private static readonly NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

        public static RedisConfiguration RedisConfig { get; private set; }

        public static IServiceCollection AddRedis(this IServiceCollection services, IConfiguration configuration)
        {
            // Register redis
            RedisConfig = configuration.GetSection("Redis").Get<RedisConfiguration>();
            if (RedisConfig == null)
            {
                Console.WriteLine("Redis Configuration: Not found");
                return services;
            }

            services.AddStackExchangeRedisExtensions<NewtonsoftSerializer>(RedisConfig);
            return services;
        }

        public static IApplicationBuilder UseRedis(this IApplicationBuilder app)
        {
            if (RedisConfig == null)
            {
                return app;
            }

            RedisConfig.Connection.ConnectionRestored += (sender, args) =>
            {
                _logger.Info("Redis connected - {0}, {1}", sender.ToString(), args.EndPoint);
            };
            RedisConfig.Connection.ConnectionFailed += (sender, args) =>
            {
                _logger.Error(args.Exception, "Failed redis connection - {0}, {1}", sender.ToString(), args.Exception.Message);
            };
            RedisConfig.Connection.ErrorMessage += (sender, args) =>
            {
                _logger.Error("Redis error Message - {0}, {1}", sender.ToString(), args.Message);
            };
            RedisConfig.Connection.InternalError += (sender, args) =>
            {
                _logger.Error(args.Exception, "Redis internal error Message - {0}, {1}", sender.ToString(), args.Origin);
            };

            if (RedisConfig.Connection.IsConnected)
            {
                Console.WriteLine("StackExchangeRedisCacheClient: Connected");
            }
            else
            {
                Console.WriteLine("StackExchangeRedisCacheClient: Disconnected");
            }

            return app;
        }
    }
}
