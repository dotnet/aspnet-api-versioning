namespace Microsoft.AspNetCore.Mvc.Versioning
{
    using FluentAssertions;
    using Http;
    using Moq;
    using Xunit;

    public class QueryStringOrHeaderApiVersionReaderTest
    {
        [Theory]
        [InlineData( "api-version", "2.1" )]
        [InlineData( "x-ms-version", "2016-07-09" )]
        public void read_should_retrieve_version_from_header( string headerName, string requestedVersion )
        {
            // arrange
            var headers = new HeaderDictionary() { [headerName] = requestedVersion };
            var request = new Mock<HttpRequest>();
            var reader = new QueryStringOrHeaderApiVersionReader() { HeaderNames = { "api-version", "x-ms-version" } };

            request.SetupGet( r => r.Query ).Returns( Mock.Of<IQueryCollection>() );
            request.SetupGet( r => r.Headers ).Returns( headers );

            // act
            var version = reader.Read( request.Object );

            // assert
            version.Should().Be( requestedVersion );
        }
    }
}
