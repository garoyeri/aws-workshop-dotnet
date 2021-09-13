namespace AwsHelloWorldWeb
{
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
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
            var mode = Configuration.GetValue<PersistenceMode>("Database:PersistenceMode");

            services.AddControllers(o =>
            {
                if (mode == PersistenceMode.Database)
                    o.Filters.Add<UnitOfWork>();
                o.Filters.Add<ExceptionFilter>();
            });

            if (inDevelopment)
            {
                services.AddSwaggerGen(c =>
                {
                    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Hello World Workshop", Version = "v1" });
                });
            }

            services.Configure<DatabaseSettings>(Configuration.GetSection("Database"));
            services.Configure<DynamoDbSettings>(Configuration.GetSection("DynamoDB"));
            services.AddDefaultAWSOptions(Configuration.GetAWSOptions());

            if (mode == PersistenceMode.DynamoDb)
            {
                services.AddDynamoDbValuesService(Configuration);
            }
            else if (mode == PersistenceMode.Database)
            {
                services.AddDatabaseValuesService(Configuration);
            }
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
