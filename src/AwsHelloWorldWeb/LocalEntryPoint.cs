using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

namespace AwsHelloWorldWeb
{
    using System.Collections.Generic;
    using Microsoft.Extensions.Configuration;

    /// <summary>
    /// The Main function can be used to run the ASP.NET Core application locally using the Kestrel webserver.
    /// </summary>
    public class LocalEntryPoint
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration((hostingContext, config) =>
                {
                    // if there is a secrets ARN configured AND we're not in development mode,
                    //  pull the secrets from the secrets manager
                    var secretsArn = config.Build().GetValue<string>("Database:ConnectionSecretArn");
                    if (secretsArn != null && !hostingContext.HostingEnvironment.IsDevelopment())
                    {
                        config.AddSecretsManager(configurator: options =>
                        {
                            options.AcceptedSecretArns = new List<string> { secretsArn };
                        });
                    }

                })
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }
}
