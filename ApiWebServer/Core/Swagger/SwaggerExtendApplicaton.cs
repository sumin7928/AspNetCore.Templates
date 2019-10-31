using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
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

        public static IServiceCollection AddCustomSwaggerGen(this IServiceCollection services)
        {
            services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new Info
                {
                    Title = "Api Controllers Docs",
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
                    Title = "Administrator Controllers Docs",
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

            return services;
        }

        public static IApplicationBuilder UseCustomSwagger(this IApplicationBuilder app)
        {
            app.UseSwagger(options =>
            {
                options.PreSerializeFilters.Add((swaggerDoc, httpReq) =>
                {
                    swaggerDoc.Host = httpReq.Host.Value;
                    IPAddress addr = httpReq.HttpContext.Connection.RemoteIpAddress;
                });
            });
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint(v1Path, "v1 Documents");
                c.SwaggerEndpoint(adminPath, "admin Documents");
            });

            SwaggerCustomDescription.Initialize();

            return app;
        }

        private static string GetXmlCommentsPath()
        {
            return Path.Combine(AppContext.BaseDirectory, xmlFile);
        }
    }
}
