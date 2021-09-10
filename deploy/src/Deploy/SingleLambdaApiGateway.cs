namespace Deploy
{
    using Amazon.CDK;
    using Amazon.CDK.AWS.APIGatewayv2;
    using Amazon.CDK.AWS.APIGatewayv2.Integrations;
    using Amazon.CDK.AWS.CertificateManager;
    using Amazon.CDK.AWS.Route53;
    using Amazon.CDK.AWS.Route53.Targets;

    public class SingleLambdaApiGatewayProps
    {
        public CfnParameter DomainName { get; set; }
        public string RootHostedZoneId { get; set; }
        public string RootHostedZoneName { get; set; }
        public LambdaProxyIntegration Integration { get; set; }
        public string NamePrefix { get; set; } = "HelloWorldWebLambda";
        public bool SkipCertificate { get; set; } = false;
    }

    public class SingleLambdaApiGateway : Construct
    {
        public IHostedZone Zone { get; }
        public Certificate Certificate { get; }
        public HttpApi Gateway { get; }

        public SingleLambdaApiGateway(Construct scope, string id, SingleLambdaApiGatewayProps props) : base(scope, id)
        {
            var fullDomainName = $"{props.DomainName.ValueAsString}.{props.RootHostedZoneName}";

            Zone = HostedZone.FromHostedZoneAttributes(this, "RootHostedZone", new HostedZoneAttributes
            {
                ZoneName = props.RootHostedZoneName,
                HostedZoneId = props.RootHostedZoneId
            });

            Certificate = props.SkipCertificate ? null : new Certificate(this, "Certificate", new DnsValidatedCertificateProps
            {
                DomainName = fullDomainName,
                HostedZone = Zone,
                Validation = CertificateValidation.FromDns(Zone)
            });
            var domain = props.SkipCertificate ? null : new DomainName(this, "CustomDomain", new DomainNameProps
            {
                DomainName = fullDomainName,
                Certificate = Certificate
            });
            Gateway = new HttpApi(this, "Gateway", new HttpApiProps
            {
                ApiName = $"{props.NamePrefix}Api",
                DefaultDomainMapping = props.SkipCertificate ? null : new DomainMappingOptions
                {
                    DomainName = domain
                }
            });

            Gateway.AddRoutes(new AddRoutesOptions
            {
                Integration = props.Integration,
                Methods = new[] { HttpMethod.GET, HttpMethod.DELETE, HttpMethod.HEAD, HttpMethod.PATCH, HttpMethod.PUT, HttpMethod.POST },
                Path = "/{proxy+}"
            });

            var dnsRecord = props.SkipCertificate ? null : new ARecord(this, "CustomAliasRecord", new ARecordProps
            {
                RecordName = props.DomainName.ValueAsString,
                Zone = Zone,
                Target = RecordTarget.FromAlias(
                    new ApiGatewayv2DomainProperties(domain.RegionalDomainName, domain.RegionalHostedZoneId))
            });
        }
    }
}