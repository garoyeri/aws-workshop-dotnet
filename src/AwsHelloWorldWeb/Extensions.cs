namespace AwsHelloWorldWeb
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Amazon;
    using Amazon.DynamoDBv2;
    using Amazon.DynamoDBv2.DataModel;
    using Amazon.Runtime;
    using Features.Values;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Options;
    using Npgsql;

    public static class Extensions
    {
        public static IServiceCollection AddDynamoDbValuesService(this IServiceCollection services, IConfiguration configuration)
        {
            // when in local development mode, the service URL is set and SOME credentials must be provided
            var dynamoOptions = configuration.GetAWSOptions("DynamoDB");
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

            services.AddSingleton<IValuesService, DynamoDbValuesService>();

            return services;
        }

        public static IServiceCollection AddDatabaseValuesService(this IServiceCollection services,
            IConfiguration configuration)
        {
            // relational database support
            services.AddDbContext<ValuesContext>(o =>
            {
                // override the connection string with values from the secrets / configuration
                var settings = configuration.GetSection("Database").Get<DatabaseSettings>();
                var connectionString =
                    new NpgsqlConnectionStringBuilder(configuration.GetConnectionString("Database"));
                connectionString.Host = settings.Host ?? connectionString.Host;
                connectionString.Port = settings.Port ?? connectionString.Port;
                connectionString.Username = settings.Username ?? connectionString.Username;
                connectionString.Password = settings.Password ?? connectionString.Password;
                connectionString.Timeout = (int)TimeSpan.FromMinutes(1).TotalSeconds;

                o.UseNpgsql(connectionString.ToString());
            });

            services.AddScoped<IValuesService, DatabaseValuesService>();

            return services;
        }
        
        public static void ConfigureSecrets(HostBuilderContext hostingContext, IConfigurationBuilder config)
        {
            // if there is a secrets ARN configured AND we're not in development mode,
            //  pull the secrets from the secrets manager
            var secretsArn = config.Build().GetValue<string>("Database:ConnectionSecretArn");
            var arn = Arn.Parse(secretsArn);
            
            if (secretsArn != null && !hostingContext.HostingEnvironment.IsDevelopment())
            {
                config.AddSecretsManager(configurator: options =>
                {
                    options.AcceptedSecretArns = new List<string> { secretsArn };
                    options.KeyGenerator = (entry, key) => $"Database:{key.MapSecretEntries(arn.Resource)}";
                });
            }
        }

        static string MapSecretEntries(this string key, string resource)
        {
            var scrubbed = key.Split(":").LastOrDefault();
            return scrubbed switch
            {
                "username" => "Username",
                "password" => "Password",
                "host" => "Host",
                "port" => "Port",
                _ => key
            };
        }
    }
}