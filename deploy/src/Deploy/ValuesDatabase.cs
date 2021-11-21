namespace Deploy
{
    using Amazon.CDK;
    using Amazon.CDK.AWS.EC2;
    using Amazon.CDK.AWS.RDS;
    using InstanceProps = Amazon.CDK.AWS.RDS.InstanceProps;

    public class ValuesDatabase : Construct
    {
        public DatabaseCluster Cluster { get; set; }
        
        public ValuesDatabase(Construct scope, string id, ValuesDatabaseProps props) : base(scope, id)
        {
            Cluster = new DatabaseCluster(this, "Database", new DatabaseClusterProps
            {
                Engine = DatabaseClusterEngine.AuroraPostgres(new AuroraPostgresClusterEngineProps
                {
                    Version = AuroraPostgresEngineVersion.VER_12_6
                }),
                Credentials = Credentials.FromGeneratedSecret("values_user"),
                InstanceProps = new InstanceProps
                {
                    InstanceType = InstanceType.Of(InstanceClass.BURSTABLE3, InstanceSize.MEDIUM),
                    Vpc = props.Vpc,
                    VpcSubnets = new SubnetSelection
                    {
#pragma warning disable CS0618
                        SubnetType = SubnetType.PRIVATE
#pragma warning restore CS0618
                    }
                },
                RemovalPolicy = props.RemovalPolicy
            });
            // not public, so it will only allow from within the VPC
            Cluster.Connections.AllowDefaultPortFromAnyIpv4();
        }
    }

    public class ValuesDatabaseProps
    {
        public IVpc Vpc { get; set; }
        public RemovalPolicy RemovalPolicy { get; set; } = RemovalPolicy.RETAIN;
    }
}