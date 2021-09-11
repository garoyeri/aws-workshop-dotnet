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
                Credentials = Credentials.FromGeneratedSecret("admin"),
                InstanceProps = new InstanceProps
                {
                    InstanceType = InstanceType.Of(InstanceClass.BURSTABLE3, InstanceSize.SMALL),
                    Vpc = props.Vpc,
                    VpcSubnets = new SubnetSelection
                    {
                        SubnetType = SubnetType.PRIVATE
                    }
                }
            });
        }
    }

    public class ValuesDatabaseProps
    {
        public IVpc Vpc { get; set; }
    }
}