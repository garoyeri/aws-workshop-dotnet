namespace AwsHelloWorldWeb
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using Amazon.Lambda.Core;
    using Features.Values;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;

    public class MigrationLambdaEntryPoint
    {
        public MigrationLambdaEntryPoint()
        {
            Host = LocalEntryPoint
                .CreateHostBuilder(Array.Empty<string>())
                .Build();
            ScopeFactory = Host.Services.GetRequiredService<IServiceScopeFactory>();
        }

        public IHost Host { get; }
        public IServiceScopeFactory ScopeFactory { get; }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Microsoft.Extensions.Hosting.Host.CreateDefaultBuilder(args)
                .ConfigureServices((hostContext, services) =>
                {
                    var startup = new MigrationsStartup(hostContext.Configuration);
                    startup.ConfigureServices(services);
                })
                .ConfigureAppConfiguration(Extensions.ConfigureSecrets);

        public class MigrationRequest
        {
        }

        public class MigrationResponse
        {
        }
        
        [LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]
        public async Task<MigrationResponse> FunctionHandlerAsync(MigrationRequest input, ILambdaContext context)
        {
            using var scope = ScopeFactory.CreateScope();

            try
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<ValuesContext>();
                await dbContext.Database.MigrateAsync();
            }
            catch (Exception e)
            {
                context.Logger.LogLine(e.ToString());
                throw;
            }

            return new MigrationResponse();
        }
    }
}