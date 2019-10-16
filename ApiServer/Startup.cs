using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ApiWebServer.Core.Swagger;
using ApiWebServer.Core.Middleware;

namespace ApiServer
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;

            // Show Config Data
            foreach (var config in Configuration.AsEnumerable())
            {
                if (config.Value != null && config.Value != string.Empty)
                {
                    Console.WriteLine($"[Config] {config.Key} - {config.Value}");
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
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
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
