namespace AwsHelloWorldWeb
{
    public class DatabaseSettings
    {
        public PersistenceMode PersistenceMode { get; set; }
        public string ConnectionSecretArn { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string Hostname { get; set; }
    }
    
    public enum PersistenceMode
    {
        DynamoDb = 1,
        Database = 2,
    }
}