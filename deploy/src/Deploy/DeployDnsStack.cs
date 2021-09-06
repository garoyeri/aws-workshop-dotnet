namespace Deploy
{
    using System;
    using Amazon.CDK;
    using Amazon.CDK.AWS.Route53;

    public class DeployDnsStack : Stack
    {
        internal DeployDnsStack(Construct scope, string id, IStackProps props = null) : base(scope, id, props)
        {
            var rootDomainName = new CfnParameter(this, "RootDomainName");
            
            var hostedZone = new PublicHostedZone(this, "RootHostedZone", new PublicHostedZoneProps
            {
                ZoneName = rootDomainName.ValueAsString
            });
            
            // output the nameservers so it can be configured with DNS provider
            new CfnOutput(this, "NameServers", new CfnOutputProps
            {
                ExportName = "RootDomainNameServers",
                Value = Fn.Join(",", hostedZone.HostedZoneNameServers ?? Array.Empty<string>())
            });
            // output the hosted zone ID so we can use elsewhere
            new CfnOutput(this, "HostedZoneId", new CfnOutputProps
            {
                ExportName = "RootDomainHostedZoneId",
                Value = hostedZone.HostedZoneId
            });
            // output the ARN too in case that's useful to have instead (usually not)
            new CfnOutput(this, "HostedZoneArn", new CfnOutputProps
            {
                ExportName = "RootDomainHostedZoneArn",
                Value = hostedZone.HostedZoneArn
            });
        }
    }
}