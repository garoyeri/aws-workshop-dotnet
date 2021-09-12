namespace Deploy
{
    using System.Collections.Generic;
    using System.ComponentModel;
    using Amazon.CDK;
    using Amazon.CDK.AWS.EC2;
    using Amazon.CDK.AWS.ECS;

    public class DeployContainerStack : Stack
    {
        public DeployContainerStack(Construct scope, string id, bool skipCertificate, IStackProps props = null) : base(scope, id, props)
        {
            const string tableNamePrefix = "HelloContainerWeb";
            
            var domainName = new CfnParameter(this, "DomainName");
            var rootHostedZoneId = Fn.ImportValue("RootDomainHostedZoneId");
            var rootHostedZoneName = Fn.ImportValue("RootDomainHostedZoneName");
            var vpcId = Fn.ImportValue("VpcId");
            var availabilityZones = Fn.ImportListValue("AvailabilityZones", 2, ",");

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
            
            var task = new HelloContainer(this, "HelloContainer", new HelloContainerProps
            {
                Cluster = cluster,
                DomainName = domainName,
                RootHostedZoneId = rootHostedZoneId,
                RootHostedZoneName = rootHostedZoneName,
                Dockerfile = "Container.Dockerfile",
                SkipCertificate = skipCertificate,
                TableNamePrefix = tableNamePrefix
            });
            
            var database = new ValuesDynamoTable(this, "Values", tableNamePrefix);
            database.Table.GrantFullAccess(task.App.TaskDefinition.TaskRole);
        }
    }
}