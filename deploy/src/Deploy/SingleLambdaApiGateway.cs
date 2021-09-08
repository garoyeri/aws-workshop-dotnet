namespace Deploy
{
    using Amazon.CDK;
    using Amazon.CDK.AWS.APIGatewayv2;
    using Amazon.CDK.AWS.APIGatewayv2.Integrations;
    using Amazon.CDK.AWS.CertificateManager;
    using Amazon.CDK.AWS.Route53;
    using Amazon.CDK.AWS.Route53.Targets;

    public class SingleLambdaApiGateway : Construct
    {
        public IHostedZone Zone { get; }
        public Certificate Certificate { get; }
        public HttpApi Gateway { get; }

        public SingleLambdaApiGateway(Construct scope, string id,
            CfnParameter domainName, CfnParameter rootHostedZoneId,
            CfnParameter rootHostedZoneName,
            LambdaProxyIntegration integration,
            bool skipCertificate = false) : base(scope, id)
        {
            var fullDomainName = $"{domainName.ValueAsString}.{rootHostedZoneName.ValueAsString}";

            Zone = HostedZone.FromHostedZoneAttributes(this, "RootHostedZone", new HostedZoneAttributes
            {
                ZoneName = rootHostedZoneName.ValueAsString,
                HostedZoneId = rootHostedZoneId.ValueAsString
            });

            Certificate = skipCertificate ? null : new Certificate(this, "Certificate", new DnsValidatedCertificateProps
            {
                DomainName = fullDomainName,
                HostedZone = Zone,
                Validation = CertificateValidation.FromDns(Zone)
            });
            var domain = skipCertificate ? null : new DomainName(this, "CustomDomain", new DomainNameProps
            {
                DomainName = fullDomainName,
                Certificate = Certificate
            });
            Gateway = new HttpApi(this, "Gateway", new HttpApiProps
            {
                ApiName = "HelloWorldWebLambdaApi",
                DefaultDomainMapping = skipCertificate ? null : new DomainMappingOptions
                {
                    DomainName = domain
                }
            });

            Gateway.AddRoutes(new AddRoutesOptions
            {
                Integration = integration,
                Methods = new[] { HttpMethod.GET, HttpMethod.DELETE, HttpMethod.HEAD, HttpMethod.PATCH, HttpMethod.PUT, HttpMethod.POST },
                Path = "/{proxy+}"
            });

            var dnsRecord = skipCertificate ? null : new ARecord(this, "CustomAliasRecord", new ARecordProps
            {
                RecordName = domainName.ValueAsString,
                Zone = Zone,
                Target = RecordTarget.FromAlias(
                    new ApiGatewayv2DomainProperties(domain.RegionalDomainName, domain.RegionalHostedZoneId))
            });
        }
    }
}