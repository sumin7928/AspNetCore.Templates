using System;
using System.Collections.Generic;
using System.IO;
using ApiServer.Core.Swagger.Docs;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Swashbuckle.AspNetCore.Swagger;

namespace ApiServer.Core.Swagger
{
    public static class SwaggerExtendApplicaton
    {
        public static readonly string xmlFile = "ApiWebServer.xml";
        public static readonly string v1Path = "/swagger/v1/swagger.json";
        public static readonly string adminPath = "/swagger/admin/swagger.json";

        public static IServiceCollection AddCustomSwaggerGen(this IServiceCollection services, IConfiguration config)
        {
            IConfigurationSection configSection = config.GetSection("Swagger");

            bool usePages = configSection.GetSection("UsePages").Get<bool>();
            if (usePages == true)
            {
                services.AddSwaggerGen(options =>
                {
                    options.SwaggerDoc("v1", new Info
                    {
                        Title = "API Controllers Docs",
                        Description = "Provides a Swagger page for testing Api controllers. It provides detailed description of each packet model and can be called directly.",

                        Contact = new Contact
                        {
                            Name = "More Infomation",
                            Email = string.Empty,
                            Url = "https://github.com/sumin7928/AspNetCore.ApiTemplate"
                        }
                    });
                    options.SwaggerDoc("admin", new Info
                    {
                        Title = "Admin API Controllers Docs",
                        Description = "Provides an administration page for administrators.",

                        Contact = new Contact
                        {
                            Name = "More Infomation",
                            Email = string.Empty,
                            Url = "https://github.com/sumin7928/AspNetCore.ApiTemplate"
                        }
                    });

                    options.IncludeXmlComments(GetXmlCommentsPath());
                    AnnotationsSwaggerGenOptionsExtensions.EnableAnnotations(options);
                });
            }

            return services;
        }

        public static IApplicationBuilder UseCustomSwagger(this IApplicationBuilder app)
        {
            IConfigurationSection configSection = app.ApplicationServices.GetService<IConfiguration>().GetSection("Swagger");

            bool usePages = configSection.GetSection("UsePages").Get<bool>();
            if (usePages == true)
            {
                app.UseSwagger(options =>
                {
                    options.PreSerializeFilters.Add((swaggerDoc, httpReq) =>
                    {
                        swaggerDoc.Host = httpReq.Host.Value;
                        var addr = httpReq.HttpContext.Connection.RemoteIpAddress;

                        if (httpReq.Path.Equals(adminPath))
                        {
                            List<string> ipList = configSection.GetSection("AllowedHosts").Get<List<string>>();
                            if (CheckAllowedIps(addr.ToString(), ipList) == false)
                            {
                                swaggerDoc.Paths = null;
                                swaggerDoc.Definitions = null;
                            }
                        }
                    });
                });
                app.UseSwaggerUI(c =>
                {
                    c.SwaggerEndpoint(v1Path, "v1 Documents");
                    c.SwaggerEndpoint(adminPath, "admin Documents");
                });

                SwaggerCustomDescription.Initialize();
            }
            return app;
        }

        private static string GetXmlCommentsPath()
        {
            return Path.Combine(AppContext.BaseDirectory, xmlFile);
        }

        private static bool CheckAllowedIps(string requestIp, List<string> ipList)
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
