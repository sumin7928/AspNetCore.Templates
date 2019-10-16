using Enyim.Caching;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Prometheus;
using System;
using ApiWebServer.Cache;
using ApiWebServer.Common;
using ApiWebServer.Core;
using ApiWebServer.Core.Extension;
using ApiWebServer.Core.Helper;
using ApiWebServer.Core.Middleware;
using ApiWebServer.Core.Swagger;
using ApiWebServer.PBTables;

namespace ApiWebServer
{
    public class Startup
    {
        private static NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;

            // Show Config
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
            // Register mvc
            services.AddMvc().SetCompatibilityVersion(Microsoft.AspNetCore.Mvc.CompatibilityVersion.Version_2_2);

            // Register configuration
            services.AddSingleton(Configuration);

            // Register memcached
            services.AddEnyimMemcached();

            // Register DBService
            services.AddDBService(Configuration);

            // Register Redis
            services.AddRedis(Configuration);

            if (AppConfig.IsRunGameServer)
            {
                // Register http client
                services.AddHttpClient();

                // Register web services
                services.AddWebService();

                // Register PB Tables
                services.AddDbContext<MaguPBTableContext>(optiopns =>
                {
                    optiopns.UseSqlServer(Configuration.GetSection("ConnectionStrings")["TableDB"]);
                });

                // Register swagger
                services.AddSwaggerGen(options => SwaggerExtendOptions.Enable(options));
            }
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            app.UseMetricServer();

            app.UseEnyimMemcached();

            // 세션 처리 Dependency 주입
            WebSessionHelper.Initialize(
                app.ApplicationServices.GetService<IConfiguration>().GetSection("GlobalSession"),
                app.ApplicationServices.GetService<IMemcachedClient>(),
                app.ApplicationServices.GetService<IDBService>());
            
            if (AppConfig.IsRunGameServer)
            {
                MaguPBTableContext context = app.ApplicationServices.GetService<MaguPBTableContext>();
                context.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;

                // 캐시 데이터 초기화
                if (CacheManager.LoadPBTable(context) == false)
                {
                    throw new Exception("PBTable load error");
                }
                Console.WriteLine("MaguPBTableContext be completed table caching");

                // 패킷 파라미터 검증 처리 초기화
                if (WebSharedLib.Core.Packet.PacketValidator.Initialize(nameof(WebSharedLib.Contents)) == false)
                {
                    throw new Exception("PacketValidator setup error");
                }
                Console.WriteLine("PacketValidator be completed initialize");

                KeyGenerator.Instance.ServerNumber = AppConfig.ServerNumber;

                SwaggerExtendApplicaton.Enable(app, Configuration.GetSection("SwaggerUI"));

                if (env.IsProduction() == false)
                {
                    app.UseMiddleware<WebInfoMiddleware>();
                }
                app.UseMiddleware<ApiMiddleware>();
            }

            if (AppConfig.IsRunChatServer)
            {
                WebSocketOptions webSocketOptions = new WebSocketOptions()
                {
                    KeepAliveInterval = TimeSpan.FromMinutes(10),
                };

                app.UseWebSockets(webSocketOptions);
                app.Map("/chat", x => x.UseMiddleware<ChatMiddleware>());
            }

            app.UseMvc();
        }
    }
}
