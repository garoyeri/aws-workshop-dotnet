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
            await _fixture.UsingValuesServiceAsync(async service =>
            {
                await service.Append("CanCreateValues()");
                var values = await service.List();
                values.ShouldContain(v => v == "CanCreateValues()");
            });
        }
    }
}