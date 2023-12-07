// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning;

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using static ApiVersionParameterLocation;
using static System.IO.Stream;

public class MediaTypeApiVersionReaderTest
{
    [Fact]
    public void read_should_return_empty_list_when_media_type_is_unspecified()
    {
        // arrange
        var reader = new MediaTypeApiVersionReader();
        var request = new Mock<HttpRequest>();

        request.SetupGet( r => r.Headers ).Returns( Mock.Of<IHeaderDictionary>() );

        // act
        var versions = reader.Read( request.Object );

        // assert
        versions.Should().BeEmpty();
    }

    [Fact]
    public void read_should_retrieve_version_from_content_type()
    {
        // arrange
        var reader = new MediaTypeApiVersionReader();
        var request = new Mock<HttpRequest>();
        var headers = new Mock<IHeaderDictionary>();

#pragma warning disable ASP0015 // Suggest using IHeaderDictionary properties
        headers.SetupGet( h => h["Content-Type"] ).Returns( new StringValues( "application/json;v=2.0" ) );
#pragma warning restore ASP0015 // Suggest using IHeaderDictionary properties
        request.SetupGet( r => r.Headers ).Returns( headers.Object );
        request.SetupProperty( r => r.Body, Null );
        request.SetupProperty( r => r.ContentLength, 0L );
        request.SetupProperty( r => r.ContentType, "application/json;v=2.0" );

        // act
        var versions = reader.Read( request.Object );

        // assert
        versions.Single().Should().Be( "2.0" );
    }

    [Fact]
    public void read_should_retrieve_version_from_accept()
    {
        // arrange
        var reader = new MediaTypeApiVersionReader();
        var request = new Mock<HttpRequest>();
        var headers = new HeaderDictionary()
        {
            ["Accept"] = "application/json;v=2.0",
        };

        request.SetupGet( r => r.Headers ).Returns( headers );

        // act
        var versions = reader.Read( request.Object );

        // assert
        versions.Single().Should().Be( "2.0" );
    }

    [Theory]
    [InlineData( new[] { "application/json;q=1;v=2.0" }, "2.0" )]
    [InlineData( new[] { "application/json;q=0.8;v=1.0", "text/plain" }, "1.0" )]
    [InlineData( new[] { "application/json;q=0.5;v=3.0", "application/xml;q=0.5;v=3.0" }, "3.0" )]
    [InlineData( new[] { "application/xml", "application/json;q=0.2;v=1.0" }, "1.0" )]
    [InlineData( new[] { "application/json", "application/xml" }, null )]
    [InlineData( new[] { "application/xml", "application/xml+atom;q=0.8;v=2.5", "application/json;q=0.2;v=1.0" }, "2.5" )]
    public void read_should_retrieve_version_from_accept_with_quality( string[] mediaTypes, string expected )
    {
        // arrange
        var reader = new MediaTypeApiVersionReader();
        var request = new Mock<HttpRequest>();
        var headers = new HeaderDictionary()
        {
            ["Accept"] = new StringValues( mediaTypes ),
        };

        request.SetupGet( r => r.Headers ).Returns( headers );

        // act
        var versions = reader.Read( request.Object );

        // assert
        versions.SingleOrDefault().Should().Be( expected );
    }

    [Fact]
    public void read_should_retrieve_version_from_content_type_and_accept()
    {
        // arrange
        var reader = new MediaTypeApiVersionReader();
        var request = new Mock<HttpRequest>();
        var mediaTypes = new[]
        {
            "application/xml",
            "application/xml+atom;q=0.8;v=1.5",
            "application/json;q=0.2;v=2.0",
        };
        var headers = new HeaderDictionary()
        {
            ["Accept"] = new StringValues( mediaTypes ),
            ["Content-Type"] = new StringValues( "application/json;v=2.0" ),
        };

        request.SetupGet( r => r.Headers ).Returns( headers );
        request.SetupProperty( r => r.Body, Null );
        request.SetupProperty( r => r.ContentLength, 0L );
        request.SetupProperty( r => r.ContentType, "application/json;v=2.0" );

        // act
        var versions = reader.Read( request.Object );

        // assert
        versions.Should().BeEquivalentTo( "1.5", "2.0" );
    }

    [Fact]
    public void read_should_retrieve_version_from_content_type_with_custom_parameter()
    {
        // arrange
        var reader = new MediaTypeApiVersionReader( "version" );
        var request = new Mock<HttpRequest>();
        var headers = new HeaderDictionary()
        {
            ["Content-Type"] = "application/json;version=1.0",
        };

        request.SetupGet( r => r.Headers ).Returns( headers );
        request.SetupProperty( r => r.Body, Null );
        request.SetupProperty( r => r.ContentLength, 0L );
        request.SetupProperty( r => r.ContentType, "application/json;version=1.0" );

        // act
        var versions = reader.Read( request.Object );

        // assert
        versions.Single().Should().Be( "1.0" );
    }

    [Fact]
    public void read_should_retrieve_version_from_accept_with_custom_parameter()
    {
        // arrange
        var reader = new MediaTypeApiVersionReader( "version" );
        var request = new Mock<HttpRequest>();
        var headers = new HeaderDictionary()
        {
            ["Accept"] = "application/json;version=3.0",
        };

        request.SetupGet( r => r.Headers ).Returns( headers );

        // act
        var versions = reader.Read( request.Object );

        // assert
        versions.Single().Should().Be( "3.0" );
    }

    [Fact]
    public void add_parameters_should_add_parameter_for_media_type()
    {
        // arrange
        var reader = new MediaTypeApiVersionReader();
        var context = new Mock<IApiVersionParameterDescriptionContext>();

        context.Setup( c => c.AddParameter( It.IsAny<string>(), It.IsAny<ApiVersionParameterLocation>() ) );

        // act
        reader.AddParameters( context.Object );

        // assert
        context.Verify( c => c.AddParameter( "v", MediaTypeParameter ), Times.Once() );
    }
}