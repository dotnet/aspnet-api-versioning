// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.Http;

using System.Net.Http.Headers;

public class MediaTypeApiVersionWriterTest
{
    [Fact]
    public void write_should_add_parameter_to_accept()
    {
        // arrange
        var request = new HttpRequestMessage( HttpMethod.Get, "http://tempuri.org" )
        {
            Headers =
            {
                Accept =
                {
                    new( "application/json" ),
                    new( "application/xml" ),
                },
            },
        };
        var writer = new MediaTypeApiVersionWriter();

        // act
        writer.Write( request, new ApiVersion( 1.0 ) );

        // assert
        request.Headers
               .Accept
               .SelectMany( accept => accept.Parameters
                                            .Where( p => p.Name == "v" )
                                            .Select( p => p.Value ) )
               .All( value => value == "1.0" )
               .Should()
               .BeTrue();
    }

    [Fact]
    public void write_should_add_custom_parameter_to_accept()
    {
        // arrange
        var request = new HttpRequestMessage( HttpMethod.Get, "http://tempuri.org" )
        {
            Headers =
            {
                Accept =
                {
                    new( "application/json" ),
                },
            },
        };
        var writer = new MediaTypeApiVersionWriter( "ver" );

        // act
        writer.Write( request, new ApiVersion( 1.0 ) );

        // assert
        request.Headers
               .Accept
               .Single()
               .Parameters
               .Single( p => p.Name == "ver" )
               .Value
               .Should()
               .Be( "1.0" );
    }

    [Fact]
    public void write_should_do_nothing_when_accept_parameter_exists()
    {
        // arrange
        var request = new HttpRequestMessage( HttpMethod.Get, "http://tempuri.org" )
        {
            Headers =
            {
                Accept =
                {
                    MediaTypeWithQualityHeaderValue.Parse( "application/json; v=2.0" ),
                },
            },
        };
        var writer = new MediaTypeApiVersionWriter();

        // act
        writer.Write( request, new ApiVersion( 1.0 ) );

        // assert
        request.Headers
               .Accept
               .Single()
               .Parameters
               .Single( p => p.Name == "v" )
               .Value
               .Should()
               .Be( "2.0" );
    }

    [Fact]
    public void write_should_add_parameter_to_content_type()
    {
        // arrange
        var request = new HttpRequestMessage( HttpMethod.Post, "http://tempuri.org" )
        {
            Content = new StreamContent( Stream.Null )
            {
                Headers =
                {
                    ContentType = new( "application/json" ),
                },
            },
        };
        var writer = new MediaTypeApiVersionWriter();

        // act
        writer.Write( request, new ApiVersion( 1.0 ) );

        // assert
        request.Content
               .Headers
               .ContentType
               .Parameters.Single( p => p.Name == "v" )
               .Value
               .Should()
               .Be( "1.0" );
    }

    [Fact]
    public void write_should_add_custom_parameter_to_content_type()
    {
        // arrange
        var request = new HttpRequestMessage( HttpMethod.Post, "http://tempuri.org" )
        {
            Content = new StreamContent( Stream.Null )
            {
                Headers =
                {
                    ContentType = new( "application/json" ),
                },
            },
        };
        var writer = new MediaTypeApiVersionWriter( "ver" );

        // act
        writer.Write( request, new ApiVersion( 1.0 ) );

        // assert
        request.Content
               .Headers
               .ContentType
               .Parameters.Single( p => p.Name == "ver" )
               .Value
               .Should()
               .Be( "1.0" );
    }

    [Fact]
    public void write_should_do_nothing_when_content_type_parameter_exists()
    {
        // arrange
        var request = new HttpRequestMessage( HttpMethod.Post, "http://tempuri.org" )
        {
            Content = new StreamContent( Stream.Null )
            {
                Headers =
                {
                    ContentType = MediaTypeHeaderValue.Parse( "application/json; v=2.0" ),
                },
            },
        };
        var writer = new MediaTypeApiVersionWriter();

        // act
        writer.Write( request, new ApiVersion( 1.0 ) );

        // assert
        request.Content
               .Headers
               .ContentType
               .Parameters.Single( p => p.Name == "v" )
               .Value
               .Should()
               .Be( "2.0" );
    }
}