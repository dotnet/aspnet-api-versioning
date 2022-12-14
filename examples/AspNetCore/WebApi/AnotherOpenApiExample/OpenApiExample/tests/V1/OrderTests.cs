namespace OpenApiExampleTests.V1
{
    using FluentAssertions;
    using Xunit.Abstractions;
    using AspVersioning.Examples.V1.Models;
    using System.Text.Json;

    [Collection(TestWebApplicationFactoryFixtureCollection.Id)]
    public class OrderTests
    {
        private readonly TestWebApplicationFactoryFixture _factory;
        private readonly ITestOutputHelper _output;
        private readonly JsonSerializerOptions _serializerOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        public OrderTests(TestWebApplicationFactoryFixture factory, ITestOutputHelper output)
        {
            _factory = factory;
            _output = output;
        }

        [Fact]
        public async Task get_order()
        {
            // Arrange
            var client = _factory.CreateClient();

            // Act
            var response = await client.GetAsync("orders/5?api-version=1.0");

            // Assert
            response.EnsureSuccessStatusCode();
            var responseJson = (await response.Content.ReadAsStringAsync());
            var order = JsonSerializer.Deserialize<Order>(responseJson, _serializerOptions);
            order.Should().NotBeNull();
            if (order == null) return; // Keep the compiler happy.
            order.Customer.Should().Be("John Doe");
            order.Id.Should().Be(5);
        }
    }
}
