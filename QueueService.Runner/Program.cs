using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using QueueService.DAL;
using System.IO;
using System.Threading.Tasks;
using System;
using QueueService.Model.Settings;
using NLog;
using NLog.Extensions.Logging;

namespace QueueService.Runner
{
    class Program
    {
        static async Task Main(string[] args)
        {

            //Load nlog settings with ability to have local files such as NLog.config.Development.json
            var nlogConfig = new ConfigurationBuilder()
            .SetBasePath(System.IO.Directory.GetCurrentDirectory())
            .AddJsonFile("NLog.config.json", optional: false, reloadOnChange: true)
            .AddJsonFile($"NLog.config.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")}.json", optional: true, reloadOnChange: true).Build();


            LogManager.Configuration = new NLogLoggingConfiguration(nlogConfig.GetSection("NLog"));



            var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")}.json", optional: true, reloadOnChange: true).Build();

            await new HostBuilder()
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddHttpClient();
                    services.AddOptions();

                    services.AddLogging(logging => {
                        logging.ClearProviders();
                        logging.SetMinimumLevel(Microsoft.Extensions.Logging.LogLevel.Trace);
                        logging.AddConsole().AddDebug();
                        logging.AddNLog(nlogConfig);
                    });

                    services.Configure<AppSettings>(configuration.GetSection("ApplicationSettings"));
                    services.Configure<DatabaseSettings>(configuration.GetSection("ApplicationSettings"));
                    services.AddSingleton<IQueueWorkerRepository, QueueWorkerRepository>();
                    services.AddSingleton<IQueueItemRepository, QueueItemRepository>();
                    services.AddHostedService<QueueItemProcessor>();
                    services.AddHostedService<QueueItemUnlocker>();
                    services.AddHostedService<QueueItemArchiver>();

                }).RunConsoleAsync();

 

        }


    }
}
