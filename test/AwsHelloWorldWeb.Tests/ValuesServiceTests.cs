namespace AwsHelloWorldWeb.Tests
{
    using System.Threading.Tasks;
    using Shouldly;
    using Xunit;

    [Collection(Fixtures.Integration)]
    public class ValuesServiceTests
    {
        private readonly IntegrationFixture _fixture;

        public ValuesServiceTests(IntegrationFixture fixture)
        {
            _fixture = fixture;
        }
        
        [Fact]
        public async Task CanCreateValues()
        {
            await _fixture.Values.Append("CanCreateValues()");
            var values = await _fixture.Values.List();
            values.ShouldContain(v => v == "CanCreateValues()");
        }
    }
}