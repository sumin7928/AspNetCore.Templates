using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;

namespace ApiWebServer.Core.Middleware
{
    public static class MiddlewareExtendApplication
    {
        public static IApplicationBuilder UseApiMiddleware(this IApplicationBuilder app, Action<Exception> exception)
        {
            app.UseWhen(context => context.Request.Path.StartsWithSegments("/api"), appBuilder =>
            {
                appBuilder.UseMiddleware<ApiMiddleware>(exception);
            });

            return app;
        }

        public static IApplicationBuilder UseWebInfoMiddleware(this IApplicationBuilder app)
        {
            app.UseWhen(context => !context.Request.Path.StartsWithSegments("/swagger"), appBuilder =>
            {
                appBuilder.UseMiddleware<WebInfoMiddleware>();
            });
            
            return app;
        }
    }
}
