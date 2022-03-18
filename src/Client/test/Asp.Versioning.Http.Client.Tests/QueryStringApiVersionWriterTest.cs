// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.Http;

using System;

public class QueryStringApiVersionWriterTest
{
    [Fact]
    public void write_should_append_query_string()
    {
        // arrange
        var request = new HttpRequestMessage( HttpMethod.Get, "http://tempuri.org" );
        var writer = new QueryStringApiVersionWriter();

        // act
        writer.Write( request, new ApiVersion( 1.0 ) );

        // assert
        request.RequestUri.Should().Be( new Uri( "http://tempuri.org?api-version=1.0" ) );
    }

    [Fact]
    public void write_should_append_custom_query_string()
    {
        // arrange
        var request = new HttpRequestMessage( HttpMethod.Get, "http://tempuri.org" );
        var writer = new QueryStringApiVersionWriter( "ver" );

        // act
        writer.Write( request, new ApiVersion( 1.0 ) );

        // assert
        request.RequestUri.Should().Be( new Uri( "http://tempuri.org?ver=1.0" ) );
    }

    [Fact]
    public void write_should_do_nothing_when_query_string_parameter_exists()
    {
        // arrange
        var request = new HttpRequestMessage( HttpMethod.Get, "http://tempuri.org?api-version=2.0" );
        var writer = new QueryStringApiVersionWriter();

        // act
        writer.Write( request, new ApiVersion( 1.0 ) );

        // assert
        request.RequestUri.Should().Be( new Uri( "http://tempuri.org?api-version=2.0" ) );
    }
}