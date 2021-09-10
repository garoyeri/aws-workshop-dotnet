using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace AwsHelloWorldWeb
{
    using Amazon.DynamoDBv2;
    using Amazon.DynamoDBv2.DataModel;
    using Amazon.Runtime;
    using Database;
    using Features.Values;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Options;
    using Microsoft.OpenApi.Models;

    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public static IConfiguration Configuration { get; private set; }

        // This method gets called by the runtime. Use this method to add services to the container
        public void ConfigureServices(IServiceCollection services)
        {
            var inDevelopment = Configuration.GetValue<string>("ASPNETCORE_ENVIRONMENT") == "Development";

            services.AddControllers(o =>
            {
                o.Filters.Add<ExceptionFilter>();
            });

            if (inDevelopment)
            {
                services.AddSwaggerGen(c =>
                {
                    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Hello World Workshop", Version = "v1" });
                });
            }

            services.Configure<DynamoDbSettings>(Configuration.GetSection("DynamoDB"));
            services.AddDefaultAWSOptions(Configuration.GetAWSOptions());
            
            services.AddScoped<ExceptionFilter>();

            // when in local development mode, the service URL is set and SOME credentials must be provided
            var dynamoOptions = Configuration.GetAWSOptions("DynamoDB");
            if (dynamoOptions.DefaultClientConfig.ServiceURL == "http://localhost:8000")
                dynamoOptions.Credentials = new BasicAWSCredentials("DUMMY", "DUMMY");
            services.AddAWSService<IAmazonDynamoDB>(dynamoOptions);

            // setup DynamoDB context, adding optional table name prefix
            services.AddSingleton<IDynamoDBContext>(p =>
            {
                var options = p.GetRequiredService<IOptions<DynamoDbSettings>>();
                var client = p.GetRequiredService<IAmazonDynamoDB>();

                return new DynamoDBContext(client, new DynamoDBContextConfig
                {
                    TableNamePrefix = options.Value.TableNamePrefix,
                    ConsistentRead = true
                });
            });
            
            services.AddSingleton<ValuesServiceDynamoDb>();
            
            // relational database support
            services.AddDbContext<ValuesContext>(o =>
            {
                o.UseNpgsql(Configuration.GetConnectionString("Database"));
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();
            
            // only turn Swagger on in development
            if (env.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI(c =>
                {
                    c.SwaggerEndpoint("v1/swagger.json", "Hello World Workshop V1");
                });
            }

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapGet("/", async context =>
                {
                    await context.Response.WriteAsync("Welcome to running ASP.NET Core on AWS Lambda");
                });
            });
        }
    }
}
