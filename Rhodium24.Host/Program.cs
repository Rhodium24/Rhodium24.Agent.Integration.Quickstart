using System;
using MediatR;
using MetalHeaven.Integration.Shared.Classes;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Rhodium24.Host.Features.AgentOutputFile;
using Serilog;

namespace Rhodium24.Host
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration()
                .Enrich.FromLogContext()
                .MinimumLevel.Debug()
                .WriteTo.Console()
                .WriteTo.File("logs\\log.txt", rollingInterval: RollingInterval.Day)
                .CreateLogger();

            try
            {
                Log.Information("Starting host");
                CreateHostBuilder(args).Build().Run();
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Host terminated unexpectedly");
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }

        private static IHostBuilder CreateHostBuilder(string[] args) =>
            Microsoft.Extensions.Hosting.Host.CreateDefaultBuilder(args)
                .UseSerilog()
                .UseWindowsService()
                .ConfigureServices((hostContext, services) =>
                {
                    // register agent settings
                    services.AddOptions<AgentSettings>().Bind(hostContext.Configuration.GetSection("AgentSettings")).ValidateDataAnnotations();

                    // register agent output file watcher service
                    services.AddHostedService<AgentOutputFileWatcherService>();

                    // register MediatR with current assembly 
                    services.AddMediatR(typeof(AgentOutputFileWatcherService).Assembly);
                });
    }
}