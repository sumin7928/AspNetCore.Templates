using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Enyim.Caching;
using Enyim.Caching.Configuration;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ApiServer.Core.Cache
{
    public static class MemcachedExtension
    {
        private static readonly NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

        public static MemcachedClientOptions MemcachedConfig { get; private set; }

        public static IServiceCollection AddMemcached(this IServiceCollection services, IConfiguration configuration)
        {
            MemcachedConfig = configuration.GetSection("Redis").Get<MemcachedClientOptions>();
            if (MemcachedConfig == null)
            {
                Console.WriteLine("Memcached Configuration: Not found");
                return services;
            }

            services.AddEnyimMemcached();
            return services;
        }

        public static IApplicationBuilder UseMemcached(this IApplicationBuilder app)
        {
            if (MemcachedConfig == null)
            {
                return app;
            }

            app.UseEnyimMemcached();

            IMemcachedClient client = app.ApplicationServices.GetService<IMemcachedClient>();
            if (client.Add("test", "value", 5))
            {
                Console.WriteLine("EnyimMemcachedClient: Connected");
            }
            else
            {
                Console.WriteLine("EnyimMemcachedClient: Disconnected");
            }

            return app;
        }
    }
}
