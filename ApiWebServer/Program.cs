using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Json;
using Microsoft.Extensions.Logging;
using NLog.Web;

namespace ApiWebServer
{
    public class Program
    {
        public static void Main(string[] args)
        {
            string envName = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
            NLogBuilder.ConfigureNLog(File.Exists($"nlog.{envName}.config") ? $"nlog.{envName}.config" : "nlog.config");

            try
            {
                CreateWebHostBuilder(args).Build().Run();
            }
            catch (Exception e)
            {
                Console.WriteLine($"Failed to run web host - message:{e.Message}");
            }
            finally
            {
                // Ensure to flush and stop internal timers/threads before application-exit (Avoid segmentation fault on Linux)
                NLog.LogManager.Shutdown();
            }
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            new WebHostBuilder()
                .ConfigureAppConfiguration((hostContext, configApp) =>
                {
                    var env = hostContext.HostingEnvironment;

                    configApp.SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                    .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: false, reloadOnChange: true)
                    .AddCommandLine(args)
                    .Build();
                })
                .ConfigureLogging((hostContext, logger) =>
                {
                    logger.ClearProviders();
                    logger.SetMinimumLevel(LogLevel.Information);
                })
                .UseNLog()
                .UseKestrel(option =>
                {
                })
                .UseStartup<Startup>();
    }
}
