using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NLog;
using NLog.Extensions.Logging;
using NLog.Web;

namespace QueueService.Api
{
	public class Program
	{
		public static void Main(string[] args)
		{
            //Load nlog settings with ability to have local files such as NLog.config.Development.json
            var nlogConfig = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("NLog.config.json", optional: false, reloadOnChange: true)
            .AddJsonFile($"NLog.config.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")}.json", optional: true, reloadOnChange: true).Build();


            LogManager.Configuration = new NLogLoggingConfiguration(nlogConfig.GetSection("NLog"));

            var logger = NLogBuilder.ConfigureNLog(LogManager.Configuration).GetCurrentClassLogger();
            try
            {
                logger.Debug("init main");
                CreateHostBuilder(args).Build().Run();
            }
            catch (Exception ex)
            {
                //NLog: catch setup errors
                logger.Error(ex, "Stopped program because of exception");
                throw;
            }
            finally
            {
                // Ensure to flush and stop internal timers/threads before application-exit (Avoid segmentation fault on Linux)
                LogManager.Shutdown();
            }
        }

		public static IHostBuilder CreateHostBuilder(string[] args) =>
			Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>()
                .ConfigureAppConfiguration((hostingContext, config) =>
                {
                    IWebHostEnvironment env = hostingContext.HostingEnvironment;

                    config.SetBasePath(Directory.GetCurrentDirectory());
                    config.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
                    config.AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true, reloadOnChange: true);
                })
               .ConfigureLogging(logging =>
               {
                   logging.ClearProviders();
                   logging.SetMinimumLevel(Microsoft.Extensions.Logging.LogLevel.Trace);
               })
               .UseNLog();  // NLog: setup NLog for Dependency injection

                });
    }
}
