// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.Http;

public class HeaderApiVersionWriterTest
{
    [Fact]
    public void write_should_append_header()
    {
        // arrange
        var request = new HttpRequestMessage( HttpMethod.Get, "http://tempuri.org" );
        var writer = new HeaderApiVersionWriter( "x-ms-version" );

        // act
        writer.Write( request, new ApiVersion( 1.0 ) );

        // assert
        request.Headers.GetValues( "x-ms-version" ).Single().Should().Be( "1.0" );
    }

    [Fact]
    public void write_should_do_nothing_when_header_exists()
    {
        // arrange
        var request = new HttpRequestMessage( HttpMethod.Get, "http://tempuri.org" );
        var writer = new HeaderApiVersionWriter( "x-ms-version" );

        request.Headers.TryAddWithoutValidation( "x-ms-version", "2.0" );

        // act
        writer.Write( request, new ApiVersion( 1.0 ) );

        // assert
        request.Headers.GetValues( "x-ms-version" ).Single().Should().Be( "2.0" );
    }
}