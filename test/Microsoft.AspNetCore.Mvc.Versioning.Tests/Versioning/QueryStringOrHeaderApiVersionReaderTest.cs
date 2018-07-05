﻿namespace Microsoft.AspNetCore.Mvc.Versioning
{
    using Extensions.Primitives;
    using FluentAssertions;
    using Http;
    using Moq;
    using System;
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
            var reader = ApiVersionReader.Combine( new QueryStringApiVersionReader(), new HeaderApiVersionReader( "api-version", "x-ms-version" ) );

            request.SetupGet( r => r.Query ).Returns( Mock.Of<IQueryCollection>() );
            request.SetupGet( r => r.Headers ).Returns( headers );

            // act
            var version = reader.Read( request.Object );

            // assert
            version.Should().Be( requestedVersion );
        }

        [Fact]
        public void read_should_throw_exception_when_ambiguous_api_versions_are_requested()
        {
            // arrange
            var query = new Mock<IQueryCollection>();
            var headers = new HeaderDictionary() { ["api-version"] = new StringValues( new[] { "1.0" } ) };
            var request = new Mock<HttpRequest>();
            var reader = ApiVersionReader.Combine( new QueryStringApiVersionReader(), new HeaderApiVersionReader( "api-version" ) );

            query.SetupGet( q => q["api-version"] ).Returns( new StringValues( "2.0" ) );
            request.SetupProperty( r => r.Query, query.Object );
            request.SetupGet( r => r.Headers ).Returns( headers );

            // act
            Action read = () => reader.Read( request.Object );

            // assert
            read.Should().Throw<AmbiguousApiVersionException>().And.ApiVersions.Should().BeEquivalentTo( "1.0", "2.0" );
        }

        [Fact]
        public void read_should_not_throw_exception_when_duplicate_api_versions_are_requested()
        {
            // arrange
            var query = new Mock<IQueryCollection>();
            var headers = new HeaderDictionary() { ["api-version"] = new StringValues( new[] { "1.0" } ) };
            var request = new Mock<HttpRequest>();
            var reader = ApiVersionReader.Combine( new QueryStringApiVersionReader(), new HeaderApiVersionReader( "api-version" ) );

            query.SetupGet( q => q["api-version"] ).Returns( new StringValues( "1.0" ) );
            request.SetupProperty( r => r.Query, query.Object );
            request.SetupGet( r => r.Headers ).Returns( headers );

            // act
            var version = reader.Read( request.Object );

            // assert
            version.Should().Be( "1.0" );
        }
    }
}