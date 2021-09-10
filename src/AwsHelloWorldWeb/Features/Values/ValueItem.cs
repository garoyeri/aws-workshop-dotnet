namespace AwsHelloWorldWeb.Features.Values
{
    using Amazon.DynamoDBv2.DataModel;

    [DynamoDBTable("Values", lowerCamelCaseProperties: true)]
    public class ValueItem
    {
        public const string DummyValue = "1";
        public const string SortedIndex = "SortedIndex";

        public ValueItem()
        {
            Dummy = DummyValue;
        }

        public ValueItem(int id, string value) : this()
        {
            Id = ValuesServiceDynamoDb.GenerateHashKey(id);
            Value = value;
        }

        public ValueItem(string hashId, string value) : this()
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