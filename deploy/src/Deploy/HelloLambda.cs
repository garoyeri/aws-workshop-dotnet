namespace Deploy
{
    using Amazon.CDK;
    using Amazon.CDK.AWS.APIGatewayv2.Integrations;
    using Amazon.CDK.AWS.EC2;
    using Amazon.CDK.AWS.Lambda;
    using Amazon.CDK.AWS.Logs;

    public class HelloLambdaProps
    {
        public string TargetPath { get; set; } = "..";
        public string FunctionHandler { get; set; } =
            "AwsHelloWorldWeb::AwsHelloWorldWeb.LambdaEntryPoint::FunctionHandlerAsync";
        public IVpc Vpc { get; set; }
        public Duration Timeout { get; set; } = Duration.Minutes(1);
    }

    public class HelloLambda : Construct
    {
        public DockerImageFunction Function { get; }
        public LambdaProxyIntegration Integration { get; }

        public HelloLambda(Construct scope, string id, HelloLambdaProps props) : base(scope, id)
        {
            Function = new DockerImageFunction(this, "Lambda", new DockerImageFunctionProps
            {
                MemorySize = 256,
                LogRetention = RetentionDays.ONE_MONTH,
                // note: this path is relative to where CDK runs from, which would be at the `deploy` folder
                Code = DockerImageCode.FromImageAsset(props.TargetPath, new AssetImageCodeProps
                {
                    Cmd = new [] {props.FunctionHandler},
                    Entrypoint = new[] { "/lambda-entrypoint.sh" }
                }),
                Timeout = props.Timeout,
                Vpc = props.Vpc
            });

            Integration = new LambdaProxyIntegration(new LambdaProxyIntegrationProps()
            {
                Handler = Function
            });
        }
    }
}