namespace Deploy
{
    using System.Collections.Generic;
    using Amazon.CDK;
    using Amazon.CDK.AWS.CertificateManager;
    using Amazon.CDK.AWS.Ecr.Assets;
    using Amazon.CDK.AWS.ECS;
    using Amazon.CDK.AWS.ECS.Patterns;
    using Amazon.CDK.AWS.Route53;

    public class HelloContainerProps
    {
        public ICluster Cluster { get; set; }
        public CfnParameter DomainName { get; set; }
        public CfnParameter RootHostedZoneId { get; set; }
        public CfnParameter RootHostedZoneName { get; set; }
        public string TargetPath { get; set; }
        public string Dockerfile { get; set; } = "Dockerfile";
        public bool SkipCertificate { get; set; } = false;
        public string TableNamePrefix = "";
    }

    public class HelloContainer : Construct
    {
        public ApplicationLoadBalancedFargateService App { get; }
        public IHostedZone Zone { get; }
        public Certificate Certificate { get; }

        public HelloContainer(Construct scope, string id, HelloContainerProps props) : base(scope, id)
        {
            var fullDomainName = $"{props.DomainName.ValueAsString}.{props.RootHostedZoneName.ValueAsString}";

            Zone = HostedZone.FromHostedZoneAttributes(this, "RootHostedZone", new HostedZoneAttributes
            {
                ZoneName = props.RootHostedZoneName.ValueAsString,
                HostedZoneId = props.RootHostedZoneId.ValueAsString
            });
            Certificate = props.SkipCertificate ? null : new Certificate(this, "Certificate", new DnsValidatedCertificateProps
            {
                DomainName = fullDomainName,
                HostedZone = Zone,
                Validation = CertificateValidation.FromDns(Zone)
            });
            
            var image = new DockerImageAsset(this, "DockerImage", new DockerImageAssetProps
            {
                Directory = props.TargetPath,
                File = props.Dockerfile
            });
            App = new ApplicationLoadBalancedFargateService(this, "Service",
                new ApplicationLoadBalancedFargateServiceProps
                {
                    Certificate = Certificate,
                    Cluster = props.Cluster,
                    Cpu = 1024,
                    MemoryLimitMiB = 512,
                    DomainZone = Zone,
                    DomainName = fullDomainName,
                    TaskImageOptions = new ApplicationLoadBalancedTaskImageOptions
                    {
                        Image = ContainerImage.FromDockerImageAsset(image),
                        ContainerName = "web",
                        Environment = new Dictionary<string, string>
                        {
                            // override the dynamodb prefix
                            { "DynamoDB__TableNamePrefix", props.TableNamePrefix }
                        }
                    }
                });
        }
    }
}