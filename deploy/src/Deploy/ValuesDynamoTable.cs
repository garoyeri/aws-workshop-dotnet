namespace Deploy
{
    using Amazon.CDK;
    using Amazon.CDK.AWS.DynamoDB;

    public class ValuesDynamoTableProps
    {
        public string TablePrefix { get; set; } = "HelloWorldWeb";
        public RemovalPolicy RemovalPolicy { get; set; } = RemovalPolicy.RETAIN;
    }

    public class ValuesDynamoTable : Construct
    {
        public ValuesDynamoTable(Construct scope, string id, ValuesDynamoTableProps props) : base(scope, id)
        {
            Table = new Table(this, "Table", new TableProps
            {
                BillingMode = BillingMode.PAY_PER_REQUEST,
                PartitionKey = new Attribute { Name = "id", Type = AttributeType.STRING },
                TableName = $"{props.TablePrefix}Values",
                RemovalPolicy = props.RemovalPolicy
            });
            
            Table.AddGlobalSecondaryIndex(new GlobalSecondaryIndexProps
            {
                IndexName = "SortedIndex",
                PartitionKey = new Attribute { Name = "dummy", Type = AttributeType.STRING },
                SortKey = new Attribute { Name = "id", Type = AttributeType.STRING },
                ProjectionType = ProjectionType.ALL
            });
        }
        
        public Table Table { get; }
    }
}