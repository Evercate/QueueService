using HMACAuthentication.Authentication;
using Microsoft.AspNetCore.Mvc;
using NLog;
using NLog.Web;
using QueueService.Api.Authentication;
using QueueService.Api.Model;
using QueueService.DAL;
using QueueService.Model.Settings;


//We need namespace, class and Main method to be able to use from integration tests
namespace QueueService.Api;

public class Program
{
    public static async Task Main(string[] args)
    {


        var logger = LogManager.Setup().LoadConfigurationFromAppSettings().GetCurrentClassLogger();
        logger.Debug("init main");

        try
        {
            var builder = WebApplication.CreateBuilder(args);

            #region Configure-Services

            // NLog: Setup NLog for Dependency injection
            builder.Logging.ClearProviders();
            builder.Logging.SetMinimumLevel(Microsoft.Extensions.Logging.LogLevel.Trace);
            builder.Host.UseNLog();

            var appSettings = builder.Configuration.GetSection("ApplicationSettings");
            builder.Services.Configure<AppSettings>(appSettings);
            builder.Services.Configure<DatabaseSettings>(appSettings);

            builder.WebHost.UseSentry();

            builder.Services.AddControllers();
            builder.Services.AddMemoryCache(); //required by Evercate.Hmac

            builder.Services.AddAuthentication(options =>
            {
                options.DefaultScheme = "HMAC";

            }).AddHMACAuthentication(options =>
            {
                //5 min is default, just setting it here for clairity. 
                options.AllowedDateDrift = TimeSpan.FromMinutes(int.TryParse(appSettings["AllowedDateDriftMinutes"], out int drift) ? drift : 5);
            }
            );

            builder.Services.AddAuthorization(options =>
            {
                options.AddPolicy("AuthenticationRequired", policy =>
                {
                    policy.RequireAuthenticatedUser();
                });
            });

            builder.Services.AddScoped<ISecretLookup, HmacSecretLookup>();
            builder.Services.AddScoped<IQueueItemRepository, QueueItemRepository>();

            builder.Services.Configure<ApiBehaviorOptions>(o =>
            {
                o.InvalidModelStateResponseFactory = actionContext =>
                    new BadRequestObjectResult(new ApiResponse
                    {
                        Success = false,
                        ErrorMessage =
                            string.Join(", ",
                                actionContext?.ModelState
                                ?.Where(modelError => modelError.Value?.Errors.Count > 0)
                                .Select(error => error.Value?.Errors.FirstOrDefault()?.ErrorMessage) ?? Enumerable.Empty<string>()
                            )
                    });
            });


            #endregion

            var app = builder.Build();

            #region configure-pipeline

            //Routing should come before authorization/authentication
            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            //Should come after authorization/authentication
            app.MapControllers();

            #endregion

            await app.RunAsync();
        }
        catch (Exception ex)
        {
            // NLog: catch setup errors
            logger.Error(ex, "Stopped program because of exception");
            throw;
        }
        finally
        {
            // Ensure to flush and stop internal timers/threads before application-exit (Avoid segmentation fault on Linux)
            LogManager.Shutdown();
        }



    }
}