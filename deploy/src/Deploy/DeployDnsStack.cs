namespace Deploy
{
    using Amazon.CDK;
    using Amazon.CDK.AWS.Route53;

    public class DeployDnsStack : Stack
    {
        internal DeployDnsStack(Construct scope, string id, IStackProps props = null) : base(scope, id, props)
        {
            var rootDomainName = new CfnParameter(this, "RootDomainName", new CfnParameterProps
            {
                Default = ""
            });

            var condition = new CfnCondition(this, "RootDomainNameCondition", new CfnConditionProps
            {
                Expression = Fn.ConditionNot(Fn.ConditionEquals(rootDomainName.ValueAsString, string.Empty))
            });

            var hostedZone = new PublicHostedZone(this, "RootHostedZone", new PublicHostedZoneProps
            {
                ZoneName = rootDomainName.ValueAsString
            });

            // only deploy the hosted zone if there is a root domain name provided
            ((CfnHostedZone)hostedZone.Node.DefaultChild).CfnOptions.Condition = condition;
        }
    }
}