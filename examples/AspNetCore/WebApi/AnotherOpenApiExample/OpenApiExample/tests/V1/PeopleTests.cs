namespace OpenApiExampleTests.V1
{
    using FluentAssertions;
    using Xunit.Abstractions;
    using AspVersioning.Examples.V1.Models;
    using System.Text.Json;

    [Collection(TestWebApplicationFactoryFixtureCollection.Id)]
    public class PeopleTests
    {
        private readonly TestWebApplicationFactoryFixture _factory;
        private readonly ITestOutputHelper _output;
        private readonly JsonSerializerOptions _serializerOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        public PeopleTests(TestWebApplicationFactoryFixture factory, ITestOutputHelper output)
        {
            _factory = factory;
            _output = output;
        }

        [Fact]
        public async Task get_people()
        {
            // Arrange
            var client = _factory.CreateClient();

            // Act
            var response = await client.GetAsync("v1.0/people/5");

            // Assert
            response.EnsureSuccessStatusCode();
            var responseJson = (await response.Content.ReadAsStringAsync());
            var person = JsonSerializer.Deserialize<Person>(responseJson, _serializerOptions);
            person.Should().NotBeNull();
            if (person == null) return; // Keep the compiler happy.
            person.FirstName.Should().Be("John");
            person.LastName.Should().Be("Doe");
            person.Id.Should().Be(5);
        }
    }
}
