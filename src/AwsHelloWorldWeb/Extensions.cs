namespace AwsHelloWorldWeb
{
    using System;
    using System.Collections.Generic;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Hosting;

    public static class Extensions
    {
        public static void ConfigureSecrets(HostBuilderContext hostingContext, IConfigurationBuilder config)
        {
            // if there is a secrets ARN configured AND we're not in development mode,
            //  pull the secrets from the secrets manager
            var secretsArn = config.Build().GetValue<string>("Database:ConnectionSecretArn");
            if (secretsArn != null && !hostingContext.HostingEnvironment.IsDevelopment())
            {
                config.AddSecretsManager(configurator: options =>
                {
                    options.AcceptedSecretArns = new List<string> { secretsArn };
                    options.KeyGenerator = (entry, key) => $"Database:{key.MapSecretEntries()}";
                });
            }
        }

        static string MapSecretEntries(this string key)
        {
            return key switch
            {
                "username" => "Username",
                "password" => "Password",
                _ => key
            };
        }
    }
}