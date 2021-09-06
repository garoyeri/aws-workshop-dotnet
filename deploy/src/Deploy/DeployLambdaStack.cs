namespace Deploy
{
    using Amazon.CDK;
    using Amazon.CDK.AWS.APIGatewayv2;
    using Amazon.CDK.AWS.CertificateManager;
    using Amazon.CDK.AWS.Route53;
    using Amazon.CDK.AWS.Route53.Targets;
    using DomainNameProps = Amazon.CDK.AWS.APIGatewayv2.DomainNameProps;

    public class DeployLambdaStack : Stack
    {
        internal DeployLambdaStack(Construct scope, string id, bool skipCertificate = false, IStackProps props = null) : base(scope, id, props)
        {
            var domainName = new CfnParameter(this, "DomainName");
            var rootHostedZoneId = new CfnParameter(this, "RootHostedZoneId");
            var rootHostedZoneName = new CfnParameter(this, "RootHostedZoneName");
            var hostedZone = HostedZone.FromHostedZoneAttributes(this, "RootHostedZone", new HostedZoneAttributes
            {
                ZoneName = rootHostedZoneName.ValueAsString,
                HostedZoneId = rootHostedZoneId.ValueAsString
            });
            var fullDomainName = $"{domainName.ValueAsString}.{rootHostedZoneName.ValueAsString}";
            
            var lambda = new HelloLambda(this, "HelloLambda", "../src/AwsHelloWorldWeb");

            var certificate = new Certificate(this, "Certificate", new DnsValidatedCertificateProps
            {
                DomainName = fullDomainName,
                HostedZone = hostedZone,
                Validation = CertificateValidation.FromDns(hostedZone)
            });
            var domain = new DomainName(this, "ApiDomain", new DomainNameProps
            {
                DomainName = fullDomainName,
                Certificate = certificate
            });
            var apiGateway = new HttpApi(this, "Api", new HttpApiProps
            {
                ApiName = "HelloWorldWebLambdaApi",
                DefaultDomainMapping = new DomainMappingOptions
                {
                    DomainName = domain
                }
            });

            apiGateway.AddRoutes(new AddRoutesOptions
            {
                Integration = lambda.Integration,
                Methods = new[] { HttpMethod.ANY },
                Path = "/{proxy+}"
            });

            var dnsRecord = new ARecord(this, "ApiAliasRecord", new ARecordProps
            {
                RecordName = domainName.ValueAsString,
                Zone = hostedZone,
                Target = RecordTarget.FromAlias(
                    new ApiGatewayv2DomainProperties(domain.RegionalDomainName, domain.RegionalHostedZoneId))
            });

            new CfnOutput(this, "ApiEndpoint", new CfnOutputProps
            {
                Value = apiGateway.ApiEndpoint
            });
        }
    }
}
