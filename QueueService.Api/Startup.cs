using HMACAuthentication.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using QueueService.Api.Authentication;
using QueueService.DAL;
using QueueService.Model.Settings;
using System;

namespace QueueService.Api
{
	public class Startup
	{
		public Startup(IConfiguration configuration)
		{
			Configuration = configuration;
		}

		public IConfiguration Configuration { get; }
		 
		// This method gets called by the runtime. Use this method to add services to the container.
		public void ConfigureServices(IServiceCollection services)
		{
			services.AddControllers();
			services.AddMemoryCache();

			var appSettings = Configuration.GetSection("ApplicationSettings");
			services.Configure<AppSettings>(appSettings);
			services.Configure<DatabaseSettings>(appSettings);

			services.AddAuthentication(options =>
			{
				options.DefaultScheme = "HMAC";

			}).AddHMACAuthentication(options =>
			{
				//5 min is default, just setting it here for clairity. 
				options.AllowedDateDrift = TimeSpan.FromMinutes(int.TryParse(appSettings["AllowedDateDriftMinutes"], out int drift) ? drift: 5);
			}
			);

			services.AddAuthorization(options =>
			{
				options.AddPolicy("AuthenticationRequired", policy =>
				{
					policy.RequireAuthenticatedUser();
				});
			});

			services.AddScoped<ISecretLookup, HmacSecretLookup>();
			services.AddScoped<IQueueItemRepository, QueueItemRepository>();
		}

		// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
		public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
		{

			if (env.IsDevelopment())
			{
				app.UseDeveloperExceptionPage();
			}

			//Routing should come before authorization/authentication
			app.UseRouting();

			app.UseAuthentication();
			app.UseAuthorization();

			//Should come after authorization/authentication
			app.UseEndpoints(endpoints =>
			{
				endpoints.MapControllers();
				endpoints.MapGet("/", async context =>
				{
					await context.Response.WriteAsync("Healthy!");
				});
			});
		}
	}
}
