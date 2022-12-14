namespace OpenApiExampleTests
{
    using System;
    using Microsoft.AspNetCore.Mvc.Testing;
    using Xunit;

    public class TestWebApplicationFactoryFixture : WebApplicationFactory<Program>
    {
        public TestWebApplicationFactoryFixture()
        {
            Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Development");
        }

    }

    [CollectionDefinition(Id)]
    public class TestWebApplicationFactoryFixtureCollection : ICollectionFixture<TestWebApplicationFactoryFixture>
    {
        public const string Id = "Test web application factory fixture collection";
    }
}
