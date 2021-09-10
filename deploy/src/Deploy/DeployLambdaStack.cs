namespace Deploy
{
    using Amazon.CDK;

    public class DeployLambdaStack : Stack
    {
        internal DeployLambdaStack(Construct scope, string id, bool skipCertificate = false, IStackProps props = null) :
            base(scope, id, props)
        {
            var domainName = new CfnParameter(this, "DomainName");
            var rootHostedZoneId = Fn.ImportValue("RootDomainHostedZoneId");
            var rootHostedZoneName = Fn.ImportValue("RootDomainHostedZoneName");

            var lambda = new HelloLambda(this, "HelloLambda", "../src/AwsHelloWorldWeb");
            var api = new SingleLambdaApiGateway(this, "Api", new SingleLambdaApiGatewayProps
            {
                DomainName = domainName,
                RootHostedZoneId = rootHostedZoneId,
                RootHostedZoneName = rootHostedZoneName,
                Integration = lambda.Integration,
                SkipCertificate = skipCertificate
            });

            new CfnOutput(this, "ApiEndpoint", new CfnOutputProps
            {
                Value = api.Gateway.ApiEndpoint
            });
        }
    }
}