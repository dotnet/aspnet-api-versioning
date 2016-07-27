namespace Microsoft.AspNetCore.Mvc.Versioning
{
    using FluentAssertions;
    using Http;
    using Moq;
    using Xunit;

    public class QueryStringApiVersionReaderTest
    {
        [Fact]
        public void read_should_retrieve_version_from_query_string()
        {
            // arrange
            var requestedVersion = "2.1";
            var query = new Mock<IQueryCollection>();
            var request = new Mock<HttpRequest>();
            var reader = new QueryStringApiVersionReader();

            query.SetupGet( q => q["api-version"] ).Returns( requestedVersion );
            request.SetupGet( r => r.Query ).Returns( query.Object );

            // act
            var version = reader.Read( request.Object );

            // assert
            version.Should().Be( requestedVersion );
        }

        [Fact]
        public void read_should_return_null_when_query_parameter_is_unspecified()
        {
            // arrange
            var query = new Mock<IQueryCollection>();
            var request = new Mock<HttpRequest>();
            var reader = new QueryStringApiVersionReader();

            query.SetupGet( q => q["api-version"] ).Returns( default( string ) );
            request.SetupGet( r => r.Query ).Returns( query.Object );

            // act
            var version = reader.Read( request.Object );

            // assert
            version.Should().BeNull();
        }

        [Fact]
        public void read_should_return_null_when_query_parameter_is_empty()
        {
            // arrange
            var query = new Mock<IQueryCollection>();
            var request = new Mock<HttpRequest>();
            var reader = new QueryStringApiVersionReader();

            query.SetupGet( q => q["api-version"] ).Returns( string.Empty );
            request.SetupGet( r => r.Query ).Returns( query.Object );

            // act
            var version = reader.Read( request.Object );

            // assert
            version.Should().Be( string.Empty );
        }
    }
}
