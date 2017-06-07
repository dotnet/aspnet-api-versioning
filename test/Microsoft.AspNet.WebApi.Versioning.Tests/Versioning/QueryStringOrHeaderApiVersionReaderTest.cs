namespace Microsoft.Web.Http.Versioning
{
    using FluentAssertions;
    using Moq;
    using System;
    using System.Net.Http;
    using Xunit;
    using static ApiVersionParameterLocation;
    using static System.Net.Http.HttpMethod;

    public class QueryStringOrHeaderApiVersionReaderTest
    {
        [Theory]
        [InlineData( "api-version", "2.1" )]
        [InlineData( "x-ms-version", "2016-07-09" )]
        public void read_should_retrieve_version_from_header( string headerName, string requestedVersion )
        {
            // arrange
            var request = new HttpRequestMessage();
            var reader = ApiVersionReader.Combine( new QueryStringApiVersionReader(), new HeaderApiVersionReader( "api-version", "x-ms-version" ) );

            request.Headers.TryAddWithoutValidation( headerName, requestedVersion );

            // act
            var version = reader.Read( request );

            // assert
            version.Should().Be( requestedVersion );
        }

        [Fact]
        public void read_should_throw_exception_when_ambiguous_api_versions_are_requested()
        {
            // arrange
            var request = new HttpRequestMessage( Get, "http://localhost/test?api-version=2.0" );
            var reader = ApiVersionReader.Combine( new QueryStringApiVersionReader(), new HeaderApiVersionReader( "api-version" ) );

            request.Headers.TryAddWithoutValidation( "api-version", "1.0" );

            // act
            Action read = () => reader.Read( request );

            // assert
            read.ShouldThrow<AmbiguousApiVersionException>().And.ApiVersions.Should().BeEquivalentTo( "1.0", "2.0" );
        }

        [Fact]
        public void read_should_not_throw_exception_when_duplicate_api_versions_are_requested()
        {
            // arrange
            var request = new HttpRequestMessage( Get, "http://localhost/test?api-version=1.0" );
            var reader = ApiVersionReader.Combine( new QueryStringApiVersionReader(), new HeaderApiVersionReader( "api-version" ) );

            request.Headers.TryAddWithoutValidation( "api-version", "1.0" );

            // act
            var version = reader.Read( request );

            // assert
            version.Should().Be( "1.0" );
        }

        [Fact]
        public void add_parameters_should_add_parameter_for_source()
        {
            // arrange
            var reader = ApiVersionReader.Combine( new QueryStringApiVersionReader(), new HeaderApiVersionReader( "api-version", "x-ms-version" ) );
            var context = new Mock<IApiVersionParameterDescriptionContext>();

            context.Setup( c => c.AddParameter( It.IsAny<string>(), It.IsAny<ApiVersionParameterLocation>() ) );

            // act
            reader.AddParmeters( context.Object );

            // assert
            context.Verify( c => c.AddParameter( "api-version", Query ), Times.Once() );
            context.Verify( c => c.AddParameter( "api-version", Header ), Times.Once() );
            context.Verify( c => c.AddParameter( "x-ms-version", Header ), Times.Once() );
        }
    }
}