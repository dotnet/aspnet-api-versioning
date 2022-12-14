namespace OpenApiExampleTests.Netural
{
    using FluentAssertions;
    using Xunit.Abstractions;

    [Collection(TestWebApplicationFactoryFixtureCollection.Id)]
    public class HealthTests
    {
        private readonly TestWebApplicationFactoryFixture _factory;
        private readonly ITestOutputHelper _output;

        public HealthTests(TestWebApplicationFactoryFixture factory, ITestOutputHelper output)
        {
            _factory = factory;
            _output = output;
        }

        [Fact]
        public async Task Ping_the_health_page()
        {
            // Arrange
            var client = _factory.CreateClient();

            // Act
            var response = await client.GetAsync("Health");

            // Assert
            response.EnsureSuccessStatusCode();
            var content = (await response.Content.ReadAsStringAsync()).Trim('"');
            content.Should().Be("I am up and running");
            _output.WriteLine(content);
        }
    }
}
