namespace Deploy
{
    using System.ComponentModel;
    using Amazon.CDK;
    using Amazon.CDK.AWS.ECS;

    public class DeployContainerStack : Stack
    {
        public DeployContainerStack(Construct scope, string id, bool skipCertificate, IStackProps props = null) : base(scope, id, props)
        {
            const string tableNamePrefix = "HelloContainerWeb";
            
            var domainName = new CfnParameter(this, "DomainName");
            var rootHostedZoneId = new CfnParameter(this, "RootHostedZoneId");
            var rootHostedZoneName = new CfnParameter(this, "RootHostedZoneName");

            var cluster = new Cluster(this, "Cluster");
            
            var task = new HelloContainer(this, "HelloContainer", new HelloContainerProps
            {
                Cluster = cluster,
                DomainName = domainName,
                RootHostedZoneId = rootHostedZoneId,
                RootHostedZoneName = rootHostedZoneName,
                TargetPath = "../src/AwsHelloWorldWeb",
                Dockerfile = "Container.Dockerfile",
                SkipCertificate = skipCertificate,
                TableNamePrefix = tableNamePrefix
            });
            
            var database = new ValuesDynamoTable(this, "Values", tableNamePrefix);
            database.Table.GrantFullAccess(task.App.TaskDefinition.TaskRole);
        }
    }
}