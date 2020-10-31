using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using AutoMapper;
using AdvertApi.Helpers;
using AdvertApi.Services;
using AdvertApi.HealthChecks;
using Microsoft.OpenApi.Models;
using Amazon.Util;
using Amazon.Runtime;
using Amazon.ServiceDiscovery;
using Amazon.ServiceDiscovery.Model;

namespace AdvertApi
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
            services.AddAutoMapper(typeof(MappingProfiles));
            services.AddTransient<IAdvertStorageService, DynamoDbAdvertStorage>();        
            services.AddControllers();
            // Add health check. Separate new get packages are available to check the health of
            // dependent resources like SQL server, S3 etc.
            // The health check is cached and the timeout of the cache is 1 min here            
            services.AddHealthChecks()
                .AddCheck<StorageHealthCheck>("Storage");
            services.AddCors(options =>
            {
                options.AddPolicy("AllOrigin", policy => policy.WithOrigins("*").AllowAnyHeader());
            });

            services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "Web Advertisement Apis",
                    Version = "version 1",
                    Contact = new OpenApiContact
                    {
                        Name = "Shyam Menon"
                    }
                });
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();

            // Add Swagger
            app.UseSwagger();
            app.UseSwaggerUI( c=>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Web Advert Api");
            });

            //await RegisterToCloudMap();

            app.UseCors();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapHealthChecks("/health");
                endpoints.MapControllers();
            });
        }

        private async Task RegisterToCloudMap()
        {
            const string serviceId = "ServiceIdOfCloudMap";
            var instanceId = EC2InstanceMetadata.InstanceId;

            if (!string.IsNullOrEmpty(instanceId))
            {
                var ipv4 = EC2InstanceMetadata.PrivateIpAddress;
                var client = new AmazonServiceDiscoveryClient();

                await client.RegisterInstanceAsync(new RegisterInstanceRequest
                {
                    InstanceId = instanceId,
                    ServiceId = serviceId,
                    Attributes = new Dictionary<string, string>
                    {
                        {"AWS_INSTANCE_IPV4", ipv4},
                        {"AWS_INSTANCE_PORT", "80" }
                    }
                });
            }
        }
    }
}
