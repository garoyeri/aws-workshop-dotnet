namespace Deploy
{
    using System.Collections.Generic;
    using Amazon.CDK;
    using Amazon.CDK.AWS.EC2;

    public class DeployDatabaseLambdaStack : Stack
    {
        public DeployDatabaseLambdaStack(Construct scope, string id,
            bool skipCertificate = false, IStackProps props = null) :
            base(scope, id, props)
        {
            var domainName = new CfnParameter(this, "DomainName");
            var rootHostedZoneId = Fn.ImportValue("RootDomainHostedZoneId");
            var rootHostedZoneName = Fn.ImportValue("RootDomainHostedZoneName");

            var vpc = Vpc.FromLookup(this, "Vpc", new VpcLookupOptions
            {
                Tags = new Dictionary<string, string>
                {
                    { "type", "workshop-primary" }
                }
            });

            var database = new ValuesDatabase(this, "Database", new ValuesDatabaseProps
            {
                Vpc = vpc
            });
            
            var lambda = new HelloLambda(this, "HelloLambda");
            var api = new SingleLambdaApiGateway(this, "Api", new SingleLambdaApiGatewayProps
            {
                DomainName = domainName,
                Integration = lambda.Integration,
                NamePrefix = "HelloDatabaseLambda",
                RootHostedZoneId = rootHostedZoneId,
                RootHostedZoneName = rootHostedZoneName,
                SkipCertificate = skipCertificate
            });
            
            // reconfigure the lambda for database mode and pass the extra configuration in the environment
            lambda.Function
                .AddEnvironment("Database__PersistenceMode", "Database")
                .AddEnvironment("Database__Hostname", database.Cluster.ClusterEndpoint.Hostname)
                .AddEnvironment("Database__ConnectionSecretArn", database.Cluster.Secret.SecretArn);

            // allow the lambda to access the secret
            database.Cluster.Secret.GrantRead(lambda.Function);
            
            new CfnOutput(this, "ApiEndpoint", new CfnOutputProps
            {
                Value = api.Gateway.ApiEndpoint
            });
            new CfnOutput(this, "Table", new CfnOutputProps
            {
                Value = database.Cluster.ClusterEndpoint.Hostname
            });
        }
    }
}