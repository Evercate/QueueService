using Microsoft.AspNetCore.Mvc;

namespace Evercate.TooEasy.Api.Controllers;

public class StatusController : ControllerBase
{
    private readonly ILogger<StatusController> logger;

    public StatusController(
        ILogger<StatusController> logger
    )
    {
        this.logger = logger;
    }

    [Route("")]
    public ActionResult Index()
    {
        try
        {
            //todo make a db call here to verify connection
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to connect to db");
            return Content("Database connection issue");
        }

        return Content($"Healthy! - Environment: {Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")}");


        //Only if you need to debug environment variables
        //return Content(string.Join("", Environment.GetEnvironmentVariables().Cast<DictionaryEntry>().OrderBy(entry => entry.Key).Select(x => $"\n{x.Key} - {x.Value}\n")));
    }

    [Route("logtest")]
    public ActionResult LogTest()
    {
        logger.LogDebug($"Logging a debug message from logtest at {DateTime.UtcNow.ToLongTimeString()}");
        logger.LogInformation($"Logging a info message from logtest at {DateTime.UtcNow.ToLongTimeString()}");
        logger.LogWarning($"Logging a warning message from logtest at {DateTime.UtcNow.ToLongTimeString()}");
        logger.LogError($"Logging a error message from logtest at {DateTime.UtcNow.ToLongTimeString()}");

        throw new ApplicationException($"throwing uncaught ApplicationException from logtest at {DateTime.UtcNow.ToLongTimeString()}");
    }
}
