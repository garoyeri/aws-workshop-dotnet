namespace Deploy
{
    using Amazon.CDK;
    using Amazon.CDK.AWS.EC2;
    using Amazon.CDK.AWS.Events.Targets;
    using Amazon.CDK.AWS.IAM;
    using Amazon.CDK.AWS.Logs;
    using LogGroupProps = Amazon.CDK.AWS.Logs.LogGroupProps;

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

            AddFlowLogsToVpc(vpc);

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

        private FlowLog AddFlowLogsToVpc(Vpc vpc)
        {
            var flowLogGroup = new LogGroup(this, "FlowLogGroup", new LogGroupProps
            {
                Retention = RetentionDays.ONE_MONTH,
                RemovalPolicy = RemovalPolicy.DESTROY
            });

            // create a role as per: https://docs.aws.amazon.com/vpc/latest/userguide/flow-logs-cwl.html
            var flowLogRole = new Role(this, "FlowLogRole", new RoleProps
            {
                AssumedBy = new ServicePrincipal("vpc-flow-logs.amazonaws.com")
            });
            flowLogRole.AddToPolicy(new PolicyStatement(new PolicyStatementProps
            {
                Actions = new[]
                {
                    "logs:CreateLogGroup",
                    "logs:CreateLogStream",
                    "logs:PutLogEvents",
                    "logs:DescribeLogGroups",
                    "logs:DescribeLogStreams"
                },
                Effect = Effect.ALLOW,
                Resources = new []{ "*" }
            }));

            // create a flow log to look for rejections
            return vpc.AddFlowLog("Flow", new FlowLogOptions
            {
                TrafficType = FlowLogTrafficType.REJECT,
                Destination = FlowLogDestination.ToCloudWatchLogs(flowLogGroup, flowLogRole)
            });
        }
    }
}