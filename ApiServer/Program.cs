using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NLog.Web;

namespace ApiServer
{
    public class Program
    {
        public static void Main(string[] args)
        {
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
            WebHost.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration((hostContext, configApp) =>
                {
                    var env = hostContext.HostingEnvironment;

                    configApp.SetBasePath(Directory.GetCurrentDirectory())
                    .AddCommandLine(args)
                    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                    .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: false, reloadOnChange: true)
                    .Build();
                })
                .ConfigureLogging((hostContext, logger) =>
                {
                    var env = hostContext.HostingEnvironment;
                    env.ConfigureNLog(File.Exists($"nlog.{env.EnvironmentName}.config") ? $"nlog.{env.EnvironmentName}.config" : "nlog.config");

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
