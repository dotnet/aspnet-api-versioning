// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning;

using System.Net.Http;
using static ApiVersionParameterLocation;

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
        var versions = reader.Read( request );

        // assert
        versions.Single().Should().Be( requestedVersion );
    }

    [Fact]
    public void read_should_return_ambiguous_api_versions()
    {
        // arrange
        var request = new HttpRequestMessage();
        var reader = new HeaderApiVersionReader( "api-version" );

        request.Headers.TryAddWithoutValidation( "api-version", new[] { "1.0", "2.0" } );

        // act
        var versions = reader.Read( request );

        // assert
        versions.Should().BeEquivalentTo( "1.0", "2.0" );
    }

    [Fact]
    public void read_should_not_throw_exception_when_duplicate_api_versions_are_requested()
    {
        // arrange
        var request = new HttpRequestMessage();
        var reader = new HeaderApiVersionReader( "api-version", "x-ms-version" );

        request.Headers.TryAddWithoutValidation( "api-version", "1.0" );
        request.Headers.TryAddWithoutValidation( "x-ms-version", "1.0" );

        // act
        var versions = reader.Read( request );

        // assert
        versions.Single().Should().Be( "1.0" );
    }

    [Fact]
    public void add_parameters_should_add_parameter_for_each_header()
    {
        // arrange
        var reader = new HeaderApiVersionReader( "api-version", "x-ms-version" );
        var context = new Mock<IApiVersionParameterDescriptionContext>();

        context.Setup( c => c.AddParameter( It.IsAny<string>(), It.IsAny<ApiVersionParameterLocation>() ) );

        // act
        reader.AddParameters( context.Object );

        // assert
        context.Verify( c => c.AddParameter( "api-version", Header ), Times.Once() );
        context.Verify( c => c.AddParameter( "x-ms-version", Header ), Times.Once() );
    }
}