using System;
using System.Linq;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ApiServer.Core.Swagger;
using ApiServer.Core.Middleware;
using ApiServer.Core.Cache;
using ApiServer.Core.DB;

namespace ApiServer
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;

            //// Show appsetting config data
            //foreach (var config in Configuration.AsEnumerable())
            //{
            //    if (config.Value != null && config.Value != string.Empty)
            //    {
            //        Console.WriteLine($"[AppSettings] {config.Key} - {config.Value}");
            //    }
            //}
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);

            // Register custom swagger
            services.AddCustomSwaggerGen();

            // Register configuration
            services.AddSingleton(Configuration);

            // Register memcached
            services.AddMemcached(Configuration);

            // Register redis
            services.AddRedis(Configuration);

            // Register relational db services from connection string
            services.AddDbService(Configuration);

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsProduction())
            {
                app.UseHttpsRedirection();
            }
            else
            {
                app.UseCustomSwagger();

                if (env.IsDevelopment())
                {
                    app.UseDeveloperExceptionPage();
                    app.UseWebInfoMiddleware();
                }
            }

            app.UseMemcached();

            app.UseRedis();

            app.UseApiMiddleware(exception =>
            {
                // api controller exception handler processing such as error notification
            });
            
            app.UseMvc();
        }
    }
}
