namespace Deploy
{
    using Amazon.CDK;
    using Amazon.CDK.AWS.EC2;

    public class DeployVpcStack : Stack
    {
        public DeployVpcStack(Construct scope, string id, IStackProps props = null) : base(scope, id, props)
        {
            var vpc = new Vpc(this, "Vpc", new VpcProps
            {
                MaxAzs = 2,
                NatGateways = 1,
            });
            Amazon.CDK.Tags.Of(vpc).Add("type", "workshop-primary");
            
            new CfnOutput(this, "VpcId", new CfnOutputProps
            {
                ExportName = "VpcId",
                Value = vpc.VpcId
            });
            new CfnOutput(this, "AvailabilityZones", new CfnOutputProps
            {
                ExportName = "AvailabilityZones",
                Value = Fn.Join(",", vpc.AvailabilityZones)
            });
        }
    }
}