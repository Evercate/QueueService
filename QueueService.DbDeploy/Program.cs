using DbUp;
using DbUp.Support;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;
using System.Reflection;

namespace QueueService.DbDeploy
{
    class Program
    {
        static int Main(string[] args)
        {

            var configuration = new ConfigurationBuilder()
           .SetBasePath(Directory.GetCurrentDirectory())
           .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
           .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")}.json", optional: true, reloadOnChange: true).Build().GetSection("ApplicationSettings");

            var connectionString = configuration["ConnectionString"];
            EnsureDatabase.For.SqlDatabase(connectionString);
            var upgrader =
                DeployChanges.To
                    .SqlDatabase(connectionString)
                    .WithScriptsEmbeddedInAssembly(Assembly.GetExecutingAssembly(), script => script.StartsWith("QueueService.DbDeploy.BeforeDeploy."), new DbUp.Engine.SqlScriptOptions { ScriptType = ScriptType.RunAlways, RunGroupOrder = 1 })
                    .WithScriptsEmbeddedInAssembly(Assembly.GetExecutingAssembly(), script => script.StartsWith("QueueService.DbDeploy.Scripts."), new DbUp.Engine.SqlScriptOptions { ScriptType = ScriptType.RunOnce, RunGroupOrder = 2 })
                    .WithScriptsEmbeddedInAssembly(Assembly.GetExecutingAssembly(), script => script.StartsWith("QueueService.DbDeploy.AfterDeploy."), new DbUp.Engine.SqlScriptOptions { ScriptType = ScriptType.RunAlways, RunGroupOrder = 3 })
                    .LogToConsole()
                    .Build();

            var result = upgrader.PerformUpgrade();

            if (!result.Successful)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(result.Error);
                Console.ResetColor();
#if DEBUG
                Console.ReadLine();
#endif
                return -1;
            }

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Success!");
            Console.ResetColor();
            return 0;
        }
    }
}
