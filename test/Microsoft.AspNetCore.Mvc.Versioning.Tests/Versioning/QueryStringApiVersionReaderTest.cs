namespace Microsoft.AspNetCore.Mvc.Versioning
{
    using Extensions.Primitives;
    using FluentAssertions;
    using Http;
    using Moq;
    using System;
    using System.Collections.Generic;
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
            request.SetupProperty( r => r.Query, query.Object );

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
            request.SetupProperty( r => r.Query, query.Object );

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
            request.SetupProperty( r => r.Query, query.Object );

            // act
            var version = reader.Read( request.Object );

            // assert
            version.Should().BeNull();
        }

        [Theory]
        [MemberData( nameof( AmbiguousQueryCollection ) )]
        public void read_should_throw_exception_when_ambiguous_api_versions_are_requested( IQueryCollection query )
        {
            // arrange
            var request = new Mock<HttpRequest>();
            var reader = new QueryStringApiVersionReader( "api-version", "version" );

            request.SetupProperty( r => r.Query, query );

            // act
            Action read = () => reader.Read( request.Object );

            // assert
            read.Should().Throw<AmbiguousApiVersionException>().And.ApiVersions.Should().BeEquivalentTo( "1.0", "2.0" );
        }

        [Theory]
        [MemberData( nameof( DuplicateQueryCollection ) )]
        public void read_should_not_throw_exception_when_duplicate_api_versions_are_requested( IQueryCollection query )
        {
            // arrange
            var request = new Mock<HttpRequest>();
            var reader = new QueryStringApiVersionReader( "api-version", "version" );

            request.SetupProperty( r => r.Query, query );

            // act
            var version = reader.Read( request.Object );

            // assert
            version.Should().Be( "1.0" );
        }

        public static IEnumerable<object[]> AmbiguousQueryCollection
        {
            get
            {
                var query = new Mock<IQueryCollection>();
                query.SetupGet( q => q["api-version"] ).Returns( new StringValues( new[] { "1.0", "2.0" } ) );
                yield return new object[] { query.Object };

                query = new Mock<IQueryCollection>();
                query.SetupGet( q => q["version"] ).Returns( new StringValues( new[] { "1.0", "2.0" } ) );
                yield return new object[] { query.Object };

                query = new Mock<IQueryCollection>();
                query.SetupGet( q => q["api-version"] ).Returns( new StringValues( "1.0" ) );
                query.SetupGet( q => q["version"] ).Returns( new StringValues( "2.0" ) );
                yield return new object[] { query.Object };
            }
        }

        public static IEnumerable<object[]> DuplicateQueryCollection
        {
            get
            {
                var query = new Mock<IQueryCollection>();
                query.SetupGet( q => q["api-version"] ).Returns( new StringValues( new[] { "1.0", "1.0" } ) );
                yield return new object[] { query.Object };

                query = new Mock<IQueryCollection>();
                query.SetupGet( q => q["version"] ).Returns( new StringValues( new[] { "1.0", "1.0" } ) );
                yield return new object[] { query.Object };

                query = new Mock<IQueryCollection>();
                query.SetupGet( q => q["api-version"] ).Returns( new StringValues( "1.0" ) );
                query.SetupGet( q => q["version"] ).Returns( new StringValues( "1.0" ) );
                yield return new object[] { query.Object };
            }
        }
    }
}