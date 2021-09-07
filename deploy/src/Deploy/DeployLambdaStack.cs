namespace Deploy
{
    using Amazon.CDK;

    public class DeployLambdaStack : Stack
    {
        internal DeployLambdaStack(Construct scope, string id, bool skipCertificate = false, IStackProps props = null) :
            base(scope, id, props)
        {
            var domainName = new CfnParameter(this, "DomainName");
            var rootHostedZoneId = new CfnParameter(this, "RootHostedZoneId");
            var rootHostedZoneName = new CfnParameter(this, "RootHostedZoneName");

            var lambda = new HelloLambda(this, "HelloLambda", "../src/AwsHelloWorldWeb");
            var api = new SingleLambdaApiGateway(this, "Api", domainName, rootHostedZoneId, rootHostedZoneName,
                lambda.Integration, skipCertificate);

            new CfnOutput(this, "ApiEndpoint", new CfnOutputProps
            {
                Value = api.Gateway.ApiEndpoint
            });
        }
    }
}