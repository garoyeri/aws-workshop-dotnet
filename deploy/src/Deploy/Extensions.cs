namespace Deploy
{
    using System.Collections.Generic;
    using System.Linq;
    using Amazon.CDK.AWS.ECS;
    using Amazon.CDK.AWS.Lambda;

    public static class Extensions
    {
        public static void UseDatabase(this Function lambda, ValuesDatabase database)
        {
            // reconfigure the lambda for database mode and pass the extra configuration in the environment
            foreach (var item in UseDatabaseEnvironment(database))
            {
                lambda.AddEnvironment(item.Key, item.Value);
            }

            // allow the lambda to access the secret
            database.Cluster.Secret.GrantRead(lambda);
        }

        public static void UseDynamoDb(this Function lambda, ValuesDynamoTable table)
        {
            // reconfigure the lambda for dynamodb mode and pass the extra configuration in the environment
            foreach (var item in UseDynamoDbEnvironment(table))
            {
                lambda.AddEnvironment(item.Key, item.Value);
            }
            
            // allow the lambda function to use the DynamoDB table
            table.Table.GrantFullAccess(lambda);
        }

        public static IDictionary<string, string> UseDatabaseEnvironment(ValuesDatabase database)
        {
            return new Dictionary<string, string>
            {
                { "Database__PersistenceMode", "Database" },
                { "Database__ConnectionSecretArn", database.Cluster.Secret.SecretArn }
            };
        }

        public static IDictionary<string, string> UseDynamoDbEnvironment(ValuesDynamoTable table)
        {
            return new Dictionary<string, string>
            {
                { "Database__PersistenceMode", "DynamoDb" },
                { "DynamoDB__TableNamePrefix", table.TablePrefix }
            };
        }

        public static void UseDatabase(this TaskDefinition container, ValuesDatabase database)
        {
            database.Cluster.Secret.GrantRead(container.TaskRole);
        }

        public static void UseDynamoDb(this TaskDefinition container, ValuesDynamoTable table)
        {
            table.Table.GrantFullAccess(container.TaskRole);
        }
    }
}