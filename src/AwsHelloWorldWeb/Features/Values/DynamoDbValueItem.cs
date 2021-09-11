namespace AwsHelloWorldWeb.Features.Values
{
    using Amazon.DynamoDBv2.DataModel;

    [DynamoDBTable("Values", lowerCamelCaseProperties: true)]
    public class DynamoDbValueItem
    {
        public const string DummyValue = "1";
        public const string SortedIndex = "SortedIndex";

        public DynamoDbValueItem()
        {
            Dummy = DummyValue;
        }

        public DynamoDbValueItem(int id, string value) : this()
        {
            Id = DynamoDbValuesService.GenerateHashKey(id);
            Value = value;
        }

        public DynamoDbValueItem(string hashId, string value) : this()
        {
            Id = hashId;
            Value = value;
        }
        
        [DynamoDBHashKey]
        [DynamoDBGlobalSecondaryIndexRangeKey(SortedIndex)]
        public string Id { get; set; }
        
        public string Value { get; set; }
        
        [DynamoDBGlobalSecondaryIndexHashKey(SortedIndex)]
        public string Dummy { get; set; }
    }
}