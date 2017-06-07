namespace Microsoft.Web.Http.Versioning
{
    using FluentAssertions;
    using Moq;
    using System;
    using System.Net.Http;
    using Xunit;
    using static ApiVersionParameterLocation;
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

        [Fact]
        public void read_should_throw_exception_when_ambiguous_api_versions_are_requested()
        {
            // arrange
            var request = new HttpRequestMessage( Get, "http://localhost/test?api-version=1.0&api-version=2.0" );
            var reader = new QueryStringApiVersionReader();

            // act
            Action read = () => reader.Read( request );

            // assert
            read.ShouldThrow<AmbiguousApiVersionException>().And.ApiVersions.Should().BeEquivalentTo( "1.0", "2.0" );
        }

        [Fact]
        public void read_should_not_throw_exception_when_duplicate_api_versions_are_requested()
        {
            // arrange
            var request = new HttpRequestMessage( Get, "http://localhost/test?api-version=1.0&api-version=1.0" );
            var reader = new QueryStringApiVersionReader();

            // act
            var version = reader.Read( request );

            // assert
            version.Should().Be( "1.0" );
        }

        [Fact]
        public void add_parameters_should_add_parameter_for_query_string()
        {
            // arrange
            var reader = new QueryStringApiVersionReader();
            var context = new Mock<IApiVersionParameterDescriptionContext>();

            context.Setup( c => c.AddParameter( It.IsAny<string>(), It.IsAny<ApiVersionParameterLocation>() ) );

            // act
            reader.AddParmeters( context.Object );

            // assert
            context.Verify( c => c.AddParameter( "api-version", Query ), Times.Once() );
        }
    }
}