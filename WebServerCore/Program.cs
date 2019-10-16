using CommandLine;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NLog.Web;
using System;
using System.IO;
using System.Net;
using ApiWebServer.Core.KestrelServer;

namespace ApiWebServer
{
    public class Program
    {
        public class Options
        {
            [Option( 'r', "runner", Required = false, HelpText = "Input server runner." )]
            public string ServerRunner { get; set; } = "game,chat";

            [Option( 'p', "port", Required = false, HelpText = "Input server port." )]
            public int ServerPort { get; set; } = 13684;

            [Option( 'n', "number", Required = false, HelpText = "Input server identity Number." )]
            public int ServerNumber { get; set; } = 1;
        }

        public static void Main(string[] args)
        {
            // Argument 셋팅
            if( ParseArgument( args ) == false )
            {
                Console.WriteLine( $"Argument Setting Error - {args}" );
                return;
            }

            // 웹 호스트 생성
            if ( BuildWebHost( out IWebHost host ) == false )
            {
                Console.WriteLine( $"Host Setting Error" );
                return;
            }

            RunWebHost( host );
        }

        private static void RunWebHost( IWebHost host )
        {
            try
            {
                if ( host != null )
                {
                    host.Run();
                }
            }
            catch ( Exception e )
            {
                Console.WriteLine( $"Failed to run web host - message:{e.Message}" );
            }
            finally
            {
                // Ensure to flush and stop internal timers/threads before application-exit (Avoid segmentation fault on Linux)
                NLog.LogManager.Shutdown();
            }
        }

        private static bool ParseArgument( string[] args )
        {
            try
            {
                Parser.Default.ParseArguments<Options>( args ).WithParsed( argument =>
                {
                    AppConfig.ServerPort = argument.ServerPort;
                    AppConfig.ServerNumber = argument.ServerNumber;

                    string serverRunner = argument.ServerRunner.ToLower();
                    AppConfig.IsRunGameServer = serverRunner.Contains( "game" );
                    AppConfig.IsRunChatServer = serverRunner.Contains( "chat" );

                } );

                Console.WriteLine( $"[{DateTime.Now}] Launcher - GameServer:{AppConfig.IsRunGameServer}, ChatServer:{AppConfig.IsRunChatServer}, " +
                    $"Port:{AppConfig.ServerPort}, Number:{AppConfig.ServerNumber}" );
            }
            catch ( Exception e )
            {
                Console.WriteLine( $"Failed to parse for arguments - message:{e.Message}" );
                return false;
            }

            return true;
        }

        private static bool BuildWebHost( out IWebHost host )
        {
            host = null;

            try
            {
                host = new WebHostBuilder()
                    .ConfigureAppConfiguration( ( hostContext, configApp ) =>
                    {
                        var env = hostContext.HostingEnvironment;

                        configApp.SetBasePath( Directory.GetCurrentDirectory() )
                        .AddJsonFile( "appsettings.json", optional: false, reloadOnChange: true )
                        .AddJsonFile( $"appsettings.{env.EnvironmentName}.json", optional: false, reloadOnChange: true )
                        .Build();
                    } )
                    .ConfigureLogging( ( hostContext, logger ) =>
                    {
                        var env = hostContext.HostingEnvironment;
                        env.ConfigureNLog( File.Exists( $"nlog.{env.EnvironmentName}.config" )? $"nlog.{env.EnvironmentName}.config" : "nlog.config" );

                        logger.ClearProviders();
                        logger.SetMinimumLevel( LogLevel.Information );
                    } )
                    .UseNLog()
                    .UseKestrel( option =>
                    {
                        option.Listen( IPAddress.IPv6Any, AppConfig.ServerPort );
                        option.Listen( IPAddress.IPv6Any, AppConfig.ServerPort + 1, listenOption =>
                        {
                            listenOption.UseHttps( KestrelServerOptionsExtensions.LoadCertificate( "certificate.pfx", "keynetmarble1@" ) );
                        } );
                    } )
                    .UseStartup<Startup>()
                    .Build();
            }
            catch ( Exception e )
            {
                Console.WriteLine( $"Failed to start game server - Message:{e.Message}" );
                return false;
            }

            return true;
        }
    }
}
