namespace AwsHelloWorldWeb.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Amazon.DynamoDBv2;
    using Amazon.DynamoDBv2.Model;
    using Features.Values;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Mvc.Testing;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Options;
    using Shouldly;

    public class IntegrationFixture : IDisposable
    {
        public IntegrationFixture()
        {
            Factory = new TestApplicationFactory();
            Configuration = Factory.Services.GetRequiredService<IConfiguration>();
            ScopeFactory = Factory.Services.GetRequiredService<IServiceScopeFactory>();

            DynamoDbSettings = Factory.Services.GetRequiredService<IOptions<DynamoDbSettings>>().Value;
            Client = Factory.Services.GetRequiredService<IAmazonDynamoDB>();
            Values = Factory.Services.GetRequiredService<ValuesServiceDynamoDb>();
        }
        
        public class TestApplicationFactory : WebApplicationFactory<Startup>
        {
            protected override void ConfigureWebHost(IWebHostBuilder builder)
            {
                builder.ConfigureAppConfiguration((_, configBuilder) =>
                {
                    configBuilder.AddInMemoryCollection(new Dictionary<string, string>
                    {
                        { "DynamoDB:ServiceURL", "http://localhost:8000" },
                        { "ASPNETCORE_ENVIRONMENT", "Development" }
                    });
                });
            }
        }
        
        public TestApplicationFactory Factory;
        public IConfiguration Configuration;
        public IServiceScopeFactory ScopeFactory;

        public DynamoDbSettings DynamoDbSettings { get; }
        public IAmazonDynamoDB Client { get; }
        
        public ValuesServiceDynamoDb Values { get; }
        
        /// <summary>
        /// Create the tables in DynamoDB
        /// </summary>
        /// <returns></returns>
        public async Task CreateTables()
        {
            // create the Values table
            var valuesTableName = DynamoDbSettings.TableNamePrefix + "Values";
            if (await WaitForTable(valuesTableName, retries: 1) != null)
            {
                await Client.DeleteTableAsync(valuesTableName);
                (await WaitForTable(valuesTableName)).ShouldBeNull();
            }

            var usersCreateTableResponse = await Client.CreateTableAsync(new CreateTableRequest
                {
                    TableName = valuesTableName,
                    KeySchema = new List<KeySchemaElement>
                    {
                        new KeySchemaElement("id", KeyType.HASH)
                    },
                    AttributeDefinitions = new List<AttributeDefinition>
                    {
                        new AttributeDefinition("id", ScalarAttributeType.S),
                        new AttributeDefinition("value", ScalarAttributeType.S),
                        new AttributeDefinition("dummy", ScalarAttributeType.S)
                    },
                    BillingMode = BillingMode.PAY_PER_REQUEST,
                    GlobalSecondaryIndexes = new List<GlobalSecondaryIndex>
                    {
                        new GlobalSecondaryIndex
                        {
                            IndexName = ValueItem.SortedIndex,
                            KeySchema = new List<KeySchemaElement>
                            {
                                new KeySchemaElement("dummy", KeyType.HASH),
                                new KeySchemaElement("id", KeyType.RANGE)
                            },
                            Projection = new Projection { ProjectionType = ProjectionType.ALL }
                        }
                    }
                }
            );
            (await WaitForTable(valuesTableName, usersCreateTableResponse.TableDescription.TableStatus))
                .ShouldBe(TableStatus.ACTIVE);
        }

        /// <summary>
        /// Wait for the table to be created.
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="initialStatus"></param>
        /// <param name="retries"></param>
        /// <param name="delay"></param>
        /// <returns></returns>
        public async Task<TableStatus> WaitForTable(string tableName, TableStatus initialStatus = null,
            int retries = 12, TimeSpan delay = default)
        {
            delay = delay == TimeSpan.Zero ? TimeSpan.FromSeconds(5) : delay;
            retries = retries <= 1 ? 1 : retries;

            try
            {
                var status = initialStatus;
                for (var i = 0; i < retries && status != TableStatus.ACTIVE; i++)
                {
                    await Task.Delay(delay);
                    var response = await Client.DescribeTableAsync(tableName);
                    status = response.Table.TableStatus;
                }

                return status;
            }
            catch (ResourceNotFoundException)
            {
                return null;
            }
        }

        public void Dispose()
        {
            Factory?.Dispose();
        }
    }
}