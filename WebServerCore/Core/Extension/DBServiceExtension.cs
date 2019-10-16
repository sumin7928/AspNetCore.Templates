using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ApiWebServer.Core.Extension
{
    public static class DBServiceExtension
    {
        public static IServiceCollection AddDBService(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddSingleton<IDBService>(new DBService(configuration.GetSection("ConnectionStrings")));

            return services;
        }
    }
}
