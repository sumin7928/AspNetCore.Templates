using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ApiServer.Core.DB
{
    public static class DbServiceExtension
    {
        private static readonly NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

        public static IServiceCollection AddDbService(this IServiceCollection services, IConfiguration configuration)
        {
            Dictionary<string, string> connStrings = configuration.GetSection("ConnectionStrings").Get<Dictionary<string, string>>();
            if (connStrings == null)
            {
                Console.WriteLine("DB ConnectionStrings: Not found");
                return services;
            }

            services.AddSingleton<IDbService>(s => new DbService(connStrings));

            return services;
        }
    }
}
