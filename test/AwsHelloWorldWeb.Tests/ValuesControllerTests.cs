namespace AwsHelloWorldWeb.Tests
{
    using System.Net;
    using System.Net.Mime;
    using System.Threading.Tasks;
    using Shouldly;
    using Xunit;

    [Collection(Fixtures.Integration)]
    public class ValuesControllerTests
    {
        private readonly IntegrationFixture _fixture;

        public ValuesControllerTests(IntegrationFixture fixture)
        {
            _fixture = fixture;
        }
        
        [Fact]
        public async Task TestGet()
        {
            var client = _fixture.Factory.CreateClient();
            var response = await client.GetAsync("/api/values");

            response.StatusCode.ShouldBe(HttpStatusCode.OK);
            response.Content.Headers.ContentType.MediaType.ShouldBe(MediaTypeNames.Application.Json);
        }
    }
}
