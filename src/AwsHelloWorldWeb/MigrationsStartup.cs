namespace AwsHelloWorldWeb
{
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;

    public class MigrationsStartup
    {
        public MigrationsStartup(IConfiguration configuration)
        {
            Configuration = configuration;
        }
        
        public static IConfiguration Configuration { get; private set; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<DatabaseSettings>(Configuration.GetSection("Database"));
            services.AddDatabaseValuesService(Configuration);
        }
    }
}