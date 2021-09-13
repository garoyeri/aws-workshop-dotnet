namespace Deploy
{
    using System.Collections.Generic;
    using Amazon.CDK;
    using Amazon.CDK.AWS.EC2;
    using Amazon.CDK.AWS.ECS;

    public class DeployDatabaseContainerStack : Stack
    {
        public DeployDatabaseContainerStack(Construct scope, string id, bool skipCertificate, IStackProps props = null) : base(scope, id, props)
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
            
            var cluster = new Cluster(this, "Cluster", new ClusterProps
            {
                Vpc = vpc
            });
            
            var database = new ValuesDatabase(this, "Database", new ValuesDatabaseProps
            {
                Vpc = vpc,
                RemovalPolicy = RemovalPolicy.DESTROY
            });

            var task = new HelloContainer(this, "HelloContainer", new HelloContainerProps
            {
                Cluster = cluster,
                DomainName = domainName,
                RootHostedZoneId = rootHostedZoneId,
                RootHostedZoneName = rootHostedZoneName,
                Dockerfile = "Container.Dockerfile",
                SkipCertificate = skipCertificate,
                Vpc = vpc,
                Environment = Extensions.UseDatabaseEnvironment(database)
            });
            
            var migration = new HelloLambda(this, "Migration", new HelloLambdaProps
            {
                FunctionHandler = "AwsHelloWorldWeb::AwsHelloWorldWeb.MigrationLambdaEntryPoint::FunctionHandlerAsync",
                Timeout = Duration.Minutes(15),
                Vpc = vpc
            });

            task.App.TaskDefinition.UseDatabase(database);
            migration.Function.UseDatabase(database);
            
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