using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HMACAuthentication.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;


namespace QueueService.ExampleMessageEndpoint
{
    public class Startup
    {

        public const string Id = "TestWorker1";
        public static readonly string Secret = "apikey";

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

            services.AddAuthentication(options =>
            {
                options.DefaultScheme = "HMAC";
            }).AddHMACAuthentication(options =>
            {
                //5 min is default, just setting it here for clairity. 
                options.AllowedDateDrift = TimeSpan.FromMinutes(5);
            }
            );

            services.AddAuthorization(options =>
            {
                options.AddPolicy("AuthenticationRequired", policy =>
                {
                    policy.RequireAuthenticatedUser();
                });
            });

            services.AddScoped<ISecretLookup, TestSecretLookup>();

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseAuthentication();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseHttpsRedirection();
            }

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }

        private class TestSecretLookup : ISecretLookup
        {
            public Task<string> LookupAsync(string id)
            {
                if (id == Startup.Id)
                    return Task.FromResult(Secret);
                else
                    return Task.FromResult<string>(null);
            }
        }
    }
}
