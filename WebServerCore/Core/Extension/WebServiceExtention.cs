using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace ApiWebServer.Core.Extension
{
    public static class WebServiceExtention
    {
        public static IServiceCollection AddWebService(this IServiceCollection services)
        {
            services.AddTransient(typeof(IWebService<,>), typeof(WebService<,>));
            return services;
        }
    }
}
