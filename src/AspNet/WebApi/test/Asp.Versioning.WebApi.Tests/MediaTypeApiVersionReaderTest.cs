// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning;

using System.Net.Http;
using System.Net.Http.Headers;
using static ApiVersionParameterLocation;
using static System.Net.Http.Headers.MediaTypeWithQualityHeaderValue;
using static System.Net.Http.HttpMethod;
using static System.Text.Encoding;

public class MediaTypeApiVersionReaderTest
{
    [Fact]
    public void read_should_return_empty_list_when_media_type_is_unspecified()
    {
        // arrange
        var reader = new MediaTypeApiVersionReader();
        var request = new HttpRequestMessage( Get, "http://tempuri.org" );

        // act
        var versions = reader.Read( request );

        // assert
        versions.Should().BeEmpty();
    }

    [Fact]
    public void read_should_retrieve_version_from_content_type()
    {
        // arrange
        var reader = new MediaTypeApiVersionReader();
        var request = new HttpRequestMessage( Post, "http://tempuri.org" )
        {
            Content = new StringContent( "{\"message\":\"test\"}", UTF8 ),
        };

        request.Content.Headers.ContentType = new MediaTypeWithQualityHeaderValue( "application/json" )
        {
            Parameters = { new NameValueHeaderValue( "v", "2.0" ) },
        };

        // act
        var versions = reader.Read( request );

        // assert
        versions.Single().Should().Be( "2.0" );
    }

    [Fact]
    public void read_should_retrieve_version_from_accept()
    {
        // arrange
        var reader = new MediaTypeApiVersionReader();
        var request = new HttpRequestMessage( Get, "http://tempuri.org" );
        var accept = new MediaTypeWithQualityHeaderValue( "application/json" )
        {
            Parameters = { new NameValueHeaderValue( "v", "2.0" ) },
        };

        request.Headers.Accept.Add( accept );

        // act
        var versions = reader.Read( request );

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
        var request = new HttpRequestMessage( Get, "http://tempuri.org" );

        foreach ( var mediaType in mediaTypes )
        {
            request.Headers.Accept.Add( Parse( mediaType ) );
        }

        // act
        var versions = reader.Read( request );

        // assert
        versions.SingleOrDefault().Should().Be( expected );
    }

    [Fact]
    public void read_should_retrieve_version_from_content_type_and_accept()
    {
        // arrange
        var reader = new MediaTypeApiVersionReader();
        var request = new HttpRequestMessage( Post, "http://tempuri.org" )
        {
            Content = new StringContent( "{\"message\":\"test\"}", UTF8 ),
        };

        request.Content.Headers.ContentType = Parse( "application/json;v=2.0" );
        request.Headers.Accept.Add( Parse( "application/xml" ) );
        request.Headers.Accept.Add( Parse( "application/xml+atom;q=0.8;v=1.5" ) );
        request.Headers.Accept.Add( Parse( "application/json;q=0.2;v=2.0" ) );

        // act
        var versions = reader.Read( request );

        // assert
        versions.Should().BeEquivalentTo( ["1.5", "2.0"] );
    }

    [Fact]
    public void read_should_retrieve_version_from_content_type_with_custom_parameter()
    {
        // arrange
        var reader = new MediaTypeApiVersionReader( "version" );
        var request = new HttpRequestMessage( Post, "http://tempuri.org" )
        {
            Content = new StringContent( "{\"message\":\"test\"}", UTF8 ),
        };

        request.Content.Headers.ContentType = new MediaTypeWithQualityHeaderValue( "application/json" )
        {
            Parameters = { new NameValueHeaderValue( "version", "1.0" ) },
        };

        // act
        var versions = reader.Read( request );

        // assert
        versions.Single().Should().Be( "1.0" );
    }

    [Fact]
    public void read_should_retrieve_version_from_accept_with_custom_parameter()
    {
        // arrange
        var reader = new MediaTypeApiVersionReader( "version" );
        var request = new HttpRequestMessage( Get, "http://tempuri.org" );
        var accept = new MediaTypeWithQualityHeaderValue( "application/json" )
        {
            Parameters = { new NameValueHeaderValue( "version", "3.0" ) },
        };

        request.Headers.Accept.Add( accept );

        // act
        var versions = reader.Read( request );

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