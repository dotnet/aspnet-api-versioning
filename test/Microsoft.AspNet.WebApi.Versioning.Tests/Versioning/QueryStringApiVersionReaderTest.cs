namespace Microsoft.Web.Http.Versioning
{
    using FluentAssertions;
    using System.Net.Http;
    using Xunit;
    using static System.Net.Http.HttpMethod;

    public class QueryStringApiVersionReaderTest
    {
        [Fact]
        public void read_should_retrieve_version_from_query_string()
        {
            // arrange
            var requestedVersion = "2.1";
            var request = new HttpRequestMessage( Get, $"http://localhost/test?api-version={requestedVersion}" );
            var reader = new QueryStringApiVersionReader();

            // act
            var version = reader.Read( request );

            // assert
            version.Should().Be( requestedVersion );
        }

        [Fact]
        public void read_should_return_null_when_query_parameter_is_unspecified()
        {
            // arrange
            var request = new HttpRequestMessage( Get, "http://localhost/test" );
            var reader = new QueryStringApiVersionReader();

            // act
            var version = reader.Read( request );

            // assert
            version.Should().BeNull();
        }

        [Fact]
        public void read_should_return_null_when_query_parameter_is_empty()
        {
            // arrange
            var request = new HttpRequestMessage( Get, "http://localhost/test?api-version=" );
            var reader = new QueryStringApiVersionReader();

            // act
            var version = reader.Read( request );

            // assert
            version.Should().BeNull();
        }
    }
}
