using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using WebAdvert.Web.Helpers;
using WebAdvert.Web.ServiceClients;
using WebAdvert.Web.Services;
using AutoMapper;
using Polly;
using System.Net.Http;
using System;
using Polly.Extensions.Http;
using System.Net;

namespace WebAdvert.Web
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
            //S3 uploader
            services.AddTransient<IFileUploader, S3FileUploader>();

            //Automapper
            services.AddAutoMapper(typeof(MappingProfiles));

            //Add HttpClient and use Polly library for exponential backoff and circuit breaker
            services.AddHttpClient<IAdvertApiClient, AdvertApiClient>().AddPolicyHandler(GetRetryPolicy())
                .AddPolicyHandler(ApplyCircuitBreakerPolicy());
            services.AddHttpClient<ISearchApiClient, SearchApiClient>().AddPolicyHandler(GetRetryPolicy())
                .AddPolicyHandler(ApplyCircuitBreakerPolicy());
            //Add AWS Cognito
            //Cognito API does not pick the policy setting. Need to check bug fix
            services.AddCognitoIdentity(config =>
            {
                config.Password = new Microsoft.AspNetCore.Identity.PasswordOptions
                {
                    RequireDigit = false,
                    RequiredLength = 6,
                    RequiredUniqueChars = 0,
                    RequireLowercase = false,
                    RequireNonAlphanumeric = false,
                    RequireUppercase = false
                };
            });

            //Set the login page
            services.ConfigureApplicationCookie(options =>
            {
                options.LoginPath = "/Accounts/Login";
            });
            services.AddControllersWithViews();
        }

        // Set the circuit breaker to break after 3 attempts for 30 seconds
        private IAsyncPolicy<HttpResponseMessage> ApplyCircuitBreakerPolicy()
        {
            return HttpPolicyExtensions.HandleTransientHttpError()
                .CircuitBreakerAsync(handledEventsAllowedBeforeBreaking: 3, TimeSpan.FromSeconds(30));
        }

        // Set retry when response from API is not found. Retry 5 times with exponential back off
        private IAsyncPolicy<HttpResponseMessage> GetRetryPolicy()
        {
            return HttpPolicyExtensions.HandleTransientHttpError()
                 .OrResult(msg => msg.StatusCode == HttpStatusCode.NotFound)
                 .WaitAndRetryAsync(retryCount: 5, sleepDurationProvider: retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }
            app.UseStaticFiles();

            app.UseRouting();

            // Use authentication from Cognito
            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
