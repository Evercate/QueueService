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
using Microsoft.AspNetCore.DataProtection;
using Sentry.Extensions.Logging;
using QueueService.Runner;


//Load here to be able to use config values here in program.cs
//In ConfigureAppConfiguration it is set so rest of application can use also
var appConfig = new ConfigurationManager()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
    .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")}.json", optional: true, reloadOnChange: true)
    .AddEnvironmentVariables()
    .Build();


var logger = NLog.LogManager.Setup().LoadConfigurationFromSection(appConfig.GetSection("NLog")).GetCurrentClassLogger();
logger.Debug("init main");

try
{


    await new HostBuilder()
        .ConfigureServices((hostContext, services) =>
        {
            services.AddHttpClient();
            services.AddOptions();
            services.Configure<AppSettings>(appConfig.GetSection("ApplicationSettings"));
            services.Configure<DatabaseSettings>(appConfig.GetSection("ApplicationSettings"));
            services.AddSingleton<ISqlConnectionFactory, SqlConnectionFactory>();
            services.AddSingleton<IQueueWorkerRepository, QueueWorkerRepository>();
            services.AddSingleton<IQueueItemRepository, QueueItemRepository>();
            services.AddHostedService<QueueItemProcessor>();
            services.AddHostedService<QueueItemUnlocker>();
            services.AddHostedService<QueueItemArchiver>();

            services.AddLogging(logging =>
            {
                // NLog: Setup NLog for Dependency injection
                logging.ClearProviders();
                logging.SetMinimumLevel(Microsoft.Extensions.Logging.LogLevel.Trace);
                logging.AddConsole().AddDebug();
                logging.AddNLog();

                IConfigurationSection sentrySection = appConfig.GetSection("Sentry");
                logging.Services.Configure<SentryLoggingOptions>(sentrySection);
                logging.AddSentry(options =>
                {
                    //For debugging sentry
                    //options.DiagnosticLogger = new FileDiagnosticLogger($"./{DateTime.UtcNow.ToString("yyyy-MM-dd")}-sentrylog.txt");
                });
            });



        }).RunConsoleAsync();

}
catch (Exception e)
{
    //NLog: catch setup errors
    logger.Error(e, "Stopped program because of exception");
    throw;
}
finally
{
    // Ensure to flush and stop internal timers/threads before application-exit (Avoid segmentation fault on Linux)
    NLog.LogManager.Shutdown();
}

