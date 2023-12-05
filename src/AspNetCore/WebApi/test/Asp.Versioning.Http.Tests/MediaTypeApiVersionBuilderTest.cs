// Copyright (c) .NET Foundation and contributors. All rights reserved.

#pragma warning disable ASP0015 // Suggest using IHeaderDictionary properties

namespace Asp.Versioning;

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using static ApiVersionParameterLocation;
using static System.IO.Stream;

public class MediaTypeApiVersionBuilderTest
{
    [Fact]
    public void read_should_return_empty_list_when_media_type_is_unspecified()
    {
        // arrange
        var reader = new MediaTypeApiVersionReaderBuilder().Build();
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
        var reader = new MediaTypeApiVersionReaderBuilder().Parameter( "v" ).Build();
        var request = new Mock<HttpRequest>();
        var headers = new Mock<IHeaderDictionary>();

        headers.SetupGet( h => h["Content-Type"] ).Returns( new StringValues( "application/json;v=2.0" ) );
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
        var reader = new MediaTypeApiVersionReaderBuilder().Parameter( "v" ).Build();
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
    [InlineData( new[] { "application/xml", "application/xml+atom;q=0.8;api.ver=2.5", "application/json;q=0.2;v=1.0" }, "2.5" )]
    public void read_should_retrieve_version_from_accept_with_quality( string[] mediaTypes, string expected )
    {
        // arrange
        var reader = new MediaTypeApiVersionReaderBuilder()
            .Parameter( "v" )
            .Parameter( "api.ver" )
            .Select( ( request, versions ) => versions.Count == 0 ? versions : new[] { versions[^1] } )
            .Build();
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
        var reader = new MediaTypeApiVersionReaderBuilder().Parameter( "v" ).Build();
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
    public void read_should_match_value_from_accept()
    {
        // arrange
        var reader = new MediaTypeApiVersionReaderBuilder().Match( @"\d+" ).Build();
        var request = new Mock<HttpRequest>();
        var headers = new HeaderDictionary()
        {
            ["Accept"] = "application/vnd-v2+json",
        };

        request.SetupGet( r => r.Headers ).Returns( headers );

        // act
        var versions = reader.Read( request.Object );

        // assert
        versions.Single().Should().Be( "2" );
    }

    [Fact]
    public void read_should_match_group_from_content_type()
    {
        // arrange
        var reader = new MediaTypeApiVersionReaderBuilder().Match( @"-v(\d+(\.\d+)?)\+" ).Build();
        var request = new Mock<HttpRequest>();
        var headers = new Mock<IHeaderDictionary>();

        headers.SetupGet( h => h["Content-Type"] ).Returns( new StringValues( "application/vnd-v2.1+json" ) );
        request.SetupGet( r => r.Headers ).Returns( headers.Object );
        request.SetupProperty( r => r.Body, Null );
        request.SetupProperty( r => r.ContentLength, 0L );
        request.SetupProperty( r => r.ContentType, "application/vnd-v2.1+json" );

        // act
        var versions = reader.Read( request.Object );

        // assert
        versions.Single().Should().Be( "2.1" );
    }

    [Fact]
    public void read_should_ignore_excluded_media_types()
    {
        // arrange
        var reader = new MediaTypeApiVersionReaderBuilder()
            .Parameter( "v" )
            .Exclude( "application/xml" )
            .Exclude( "application/xml+atom" )
            .Build();
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
        versions.Single().Should().Be( "2.0" );
    }

    [Fact]
    public void read_should_only_retrieve_included_media_types()
    {
        // arrange
        var reader = new MediaTypeApiVersionReaderBuilder()
            .Parameter( "v" )
            .Include( "application/json" )
            .Build();
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
        versions.Single().Should().Be( "2.0" );
    }

    [Fact]
    public void read_should_assume_version_from_single_parameter_in_media_type_template()
    {
        // arrange
        var reader = new MediaTypeApiVersionReaderBuilder()
            .Template( "application/vnd-v{ver}+json" )
            .Build();
        var request = new Mock<HttpRequest>();
        var headers = new HeaderDictionary()
        {
            ["Accept"] = "application/vnd-v1+json",
        };

        request.SetupGet( r => r.Headers ).Returns( headers );

        // act
        var versions = reader.Read( request.Object );

        // assert
        versions.Single().Should().Be( "1" );
    }

    [Theory]
    [InlineData( "application/vnd-v{v}+json", "v", "application/vnd-v2.1+json", "2.1" )]
    [InlineData( "application/vnd-v{ver}+json", "ver", "application/vnd-v2022-11-01+json", "2022-11-01" )]
    [InlineData( "application/vnd-{version}+xml", "version", "application/vnd-1.1-beta+xml", "1.1-beta" )]
    public void read_should_retrieve_version_from_media_type_template(
        string template,
        string parameterName,
        string mediaType,
        string expected )
    {
        // arrange
        var reader = new MediaTypeApiVersionReaderBuilder().Template( template, parameterName ).Build();
        var request = new Mock<HttpRequest>();
        var headers = new HeaderDictionary()
        {
            ["Accept"] = mediaType,
        };

        request.SetupGet( r => r.Headers ).Returns( headers );

        // act
        var versions = reader.Read( request.Object );

        // assert
        versions.Single().Should().Be( expected );
    }

    [Fact]
    public void read_should_throw_exception_with_multiple_parameters_and_no_name()
    {
        // arrange
        var reader = new MediaTypeApiVersionReaderBuilder();

        // act
        var template = () => reader.Template( "application/vnd-v{ver}+json+{other}" );

        // assert
        template.Should().Throw<ArgumentException>().And
                .ParamName.Should().Be( nameof( template ) );
    }

    [Fact]
    public void read_should_return_empty_list_when_template_does_not_match()
    {
        // arrange
        var reader = new MediaTypeApiVersionReaderBuilder()
            .Template( "application/vnd-v{ver}+json", "ver" )
            .Build();
        var request = new Mock<HttpRequest>();
        var headers = new HeaderDictionary()
        {
            ["Accept"] = "text/plain",
        };

        request.SetupGet( r => r.Headers ).Returns( headers );

        // act
        var versions = reader.Read( request.Object );

        // assert
        versions.Should().BeEmpty();
    }

    [Fact]
    public void read_should_select_first_version()
    {
        // arrange
        var reader = new MediaTypeApiVersionReaderBuilder()
            .Parameter( "v" )
            .SelectFirstOrDefault()
            .Build();
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
        versions.Single().Should().Be( "1.5" );
    }

    [Fact]
    public void read_should_select_last_version()
    {
        // arrange
        var reader = new MediaTypeApiVersionReaderBuilder()
            .Parameter( "v" )
            .SelectLastOrDefault()
            .Build();
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
        versions.Single().Should().Be( "2.0" );
    }

    [Fact]
    public void add_parameters_should_add_parameter_for_media_type()
    {
        // arrange
        var reader = new MediaTypeApiVersionReaderBuilder().Parameter( "v" ).Build();
        var context = new Mock<IApiVersionParameterDescriptionContext>();

        context.Setup( c => c.AddParameter( It.IsAny<string>(), It.IsAny<ApiVersionParameterLocation>() ) );

        // act
        reader.AddParameters( context.Object );

        // assert
        context.Verify( c => c.AddParameter( "v", MediaTypeParameter ), Times.Once() );
    }
}