namespace AwsHelloWorldWeb.Tests
{
    using Xunit;

    public static class Fixtures
    {
        public const string Integration = "Integration Tests";
    }

    [CollectionDefinition(Fixtures.Integration)]
    public class IntegrationTestCollection : ICollectionFixture<IntegrationFixture>
    {
    }
}