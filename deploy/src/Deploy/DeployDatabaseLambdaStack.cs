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
                Vpc = vpc,
                RemovalPolicy = RemovalPolicy.DESTROY
            });

            var migration = new HelloLambda(this, "Migration", new HelloLambdaProps
            {
                FunctionHandler = "AwsHelloWorldWeb::AwsHelloWorldWeb.MigrationLambdaEntryPoint::FunctionHandlerAsync",
                Timeout = Duration.Minutes(15),
                Vpc = vpc
            });

            var lambda = new HelloLambda(this, "HelloLambda", new HelloLambdaProps
            {
                Vpc = vpc
            });
            var api = new SingleLambdaApiGateway(this, "Api", new SingleLambdaApiGatewayProps
            {
                DomainName = domainName,
                Integration = lambda.Integration,
                NamePrefix = "HelloDatabaseLambda",
                RootHostedZoneId = rootHostedZoneId,
                RootHostedZoneName = rootHostedZoneName,
                SkipCertificate = skipCertificate
            });

            lambda.Function.UseDatabase(database);
            migration.Function.UseDatabase(database);

            new CfnOutput(this, "ApiEndpoint", new CfnOutputProps
            {
                Value = api.Gateway.ApiEndpoint
            });
            new CfnOutput(this, "Table", new CfnOutputProps
            {
                Value = database.Cluster.ClusterEndpoint.Hostname
            });
            new CfnOutput(this, "MigrationFunctionArn", new CfnOutputProps
            {
                Value = migration.Function.FunctionArn
            });
        }
    }
}