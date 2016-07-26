namespace Microsoft.Web.Http.Versioning
{
    using FluentAssertions;
    using System.Net.Http;
    using Xunit;

    public class HeaderApiVersionReaderTest
    {
        [Theory]
        [InlineData( "api-version", "2.1" )]
        [InlineData( "x-ms-version", "2016-07-09" )]
        public void read_should_retrieve_version_from_header( string headerName, string requestedVersion )
        {
            // arrange
            var request = new HttpRequestMessage();
            var reader = new HeaderApiVersionReader( "api-version", "x-ms-version" );

            request.Headers.TryAddWithoutValidation( headerName, requestedVersion );

            // act
            var version = reader.Read( request );

            // assert
            version.Should().Be( requestedVersion );
        }
    }
}
