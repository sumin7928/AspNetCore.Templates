using System;
using System.Linq;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ApiWebServer.Core.Swagger;
using ApiWebServer.Core.Middleware;
using ApiWebServer.Core.Cache;

namespace ApiWebServer
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;

            // Show appsetting config data
            foreach (var config in Configuration.AsEnumerable())
            {
                if (config.Value != null && config.Value != string.Empty)
                {
                    Console.WriteLine($"[AppSettings] {config.Key} - {config.Value}");
                }
            }
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);

            // Register configuration
            services.AddSingleton(Configuration);

            // Register custom swagger
            services.AddCustomSwaggerGen(Configuration);

            // Register memcached
            services.AddMemcached(Configuration);

            // Register Redis
            services.AddRedis(Configuration);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            app.UseMemcached();

            //app.UseHttpsRedirection();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseWebInfoMiddleware();
            }

            app.UseCustomSwagger();

            app.UseApiMiddleware(exception =>
            {
                // api controller exception handler processing
            });

            app.UseMvc();
        }
    }
}
