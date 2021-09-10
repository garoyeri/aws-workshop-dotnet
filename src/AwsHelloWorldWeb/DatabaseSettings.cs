namespace AwsHelloWorldWeb
{
    public class DatabaseSettings
    {
        public PersistenceMode PersistenceMode { get; set; }
        public string DatabaseSecretArn { get; set; }
    }
    
    public enum PersistenceMode
    {
        DynamoDb = 1,
        Database = 2,
    }
}