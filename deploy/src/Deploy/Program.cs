namespace Deploy
{
    using Amazon.CDK;
    
    sealed class Program
    {
        public static void Main(string[] args)
        {
            // if you're not planning on using custom domains, change this to `true`
            var skipCertificate = false;
            var app = new App();
            Tags.Of(app).Add("workshop", "https://github.com/garoyeri/aws-workshop-dotnet");

            new DeployDnsStack(app, "DeployDnsStack", MakeStackProps());
            new DeployVpcStack(app, "DeployVpcStack", MakeStackProps());
            
            new DeployLambdaStack(app, "DeployLambdaStack", skipCertificate, MakeStackProps());
            new DeployContainerStack(app, "DeployContainerStack", skipCertificate, MakeStackProps());
            
            new DeployDatabaseLambdaStack(app, "DeployDatabaseLambdaStack", skipCertificate, MakeStackProps());
            new DeployDatabaseContainerStack(app, "DeployDatabaseContainerStack", skipCertificate, MakeStackProps());

            app.Synth();
        }

        private static IStackProps MakeStackProps() =>
            new StackProps
            {
                // If you don't specify 'env', this stack will be environment-agnostic.
                // Account/Region-dependent features and context lookups will not work,
                // but a single synthesized template can be deployed anywhere.

                // Uncomment the next block to specialize this stack for the AWS Account
                // and Region that are implied by the current CLI configuration.

                Env = new Environment
                {
                    Account = System.Environment.GetEnvironmentVariable("CDK_DEFAULT_ACCOUNT"),
                    Region = System.Environment.GetEnvironmentVariable("CDK_DEFAULT_REGION"),
                }


                // Uncomment the next block if you know exactly what Account and Region you
                // want to deploy the stack to.
                /*
                Env = new Amazon.CDK.Environment
                {
                    Account = "123456789012",
                    Region = "us-east-1",
                }
                */

                // For more information, see https://docs.aws.amazon.com/cdk/latest/guide/environments.html
            };
    }
}