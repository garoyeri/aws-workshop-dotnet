namespace Deploy
{
    using Amazon.CDK;
    using Amazon.CDK.AWS.APIGatewayv2.Integrations;
    using Amazon.CDK.AWS.Lambda;

    public class HelloLambda : Construct
    {
        public DockerImageFunction Function { get; }
        public LambdaProxyIntegration Integration { get; }

        public HelloLambda(Construct scope, string id, string targetPath) : base(scope, id)
        {
            Function = new DockerImageFunction(this, "Lambda", new DockerImageFunctionProps
            {
                // note: this path is relative to where CDK runs from, which would be at the `deploy` folder
                Code = DockerImageCode.FromImageAsset(targetPath, new AssetImageCodeProps
                {
                    Cmd = new [] {"AwsHelloWorldWeb::AwsHelloWorldWeb.LambdaEntryPoint::FunctionHandlerAsync"},
                    Entrypoint = new[] { "/lambda-entrypoint.sh" }
                })
            });

            Integration = new LambdaProxyIntegration(new LambdaProxyIntegrationProps()
            {
                Handler = Function
            });
        }
    }
}