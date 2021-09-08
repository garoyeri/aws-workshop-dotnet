namespace Deploy
{
    using Amazon.CDK;
    using Amazon.CDK.AWS.DynamoDB;

    public class ValuesDynamoTable : Construct
    {
        public ValuesDynamoTable(Construct scope, string id) : base(scope, id)
        {
            Table = new Table(this, "Table", new TableProps
            {
                BillingMode = BillingMode.PAY_PER_REQUEST,
                PartitionKey = new Attribute { Name = "id", Type = AttributeType.STRING },
                TableName = "HelloWorldWebValues"
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