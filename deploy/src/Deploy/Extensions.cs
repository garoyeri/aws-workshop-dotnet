namespace Deploy
{
    using Amazon.CDK.AWS.Lambda;

    public static class Extensions
    {
        public static void UseDatabase(this Function lambda, ValuesDatabase database)
        {
            // reconfigure the lambda for database mode and pass the extra configuration in the environment
            lambda
                .AddEnvironment("Database__PersistenceMode", "Database")
                .AddEnvironment("Database__ConnectionSecretArn", database.Cluster.Secret.SecretArn);

            // allow the lambda to access the secret
            database.Cluster.Secret.GrantRead(lambda);
        }
        
    }
}