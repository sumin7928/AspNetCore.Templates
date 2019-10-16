using System.Collections.Generic;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;

namespace ApiWebServer.Core.Swagger
{
    public static class SwaggerExtendApplicaton
    {
        public static readonly string clientPath = "/swagger/client/swagger.json";
        public static readonly string adminPath = "/swagger/admin/swagger.json";

        public static void Enable(IApplicationBuilder app, IConfigurationSection config)
        {
            app.UseSwagger(c =>
            {
                c.PreSerializeFilters.Add((swaggerDoc, httpReq) =>
                {
                    swaggerDoc.Host = httpReq.Host.Value;
                    var addr = httpReq.HttpContext.Connection.RemoteIpAddress;
                    bool allowed = false;

                    if (httpReq.Path.Equals(clientPath))
                    {
                        List<string> ipList = config.GetSection("ClientAllowedIps").Get<List<string>>();
                        allowed = CheckAllowedIps(addr.ToString(), ipList);
                    }
                    else if (httpReq.Path.Equals(adminPath))
                    {
                        List<string> ipList = config.GetSection("AdminAllowedIps").Get<List<string>>();
                        allowed = CheckAllowedIps(addr.ToString(), ipList);
                    }

                    if (allowed == false)
                    {
                        swaggerDoc.Paths = null;
                        swaggerDoc.Definitions = null;
                    }
                });
            });
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint(clientPath, "Client Documents");
                c.SwaggerEndpoint(adminPath, "Admin Documents");
            });
        }

        private static bool CheckAllowedIps( string requestIp, List<string> ipList)
        {
            foreach (string ip in ipList)
            {
                if (requestIp.Contains(ip))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
