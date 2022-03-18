// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.Http;

public class UrlSegmentApiVersionWriterTest
{
    [Fact]
    public void write_should_replace_token_in_url()
    {
        // arrange
        var request = new HttpRequestMessage( HttpMethod.Get, "http://localhost/api/v{ver}/test" );
        var writer = new UrlSegmentApiVersionWriter( "{ver}" );

        // act
        writer.Write( request, new ApiVersion( 1 ) );

        // assert
        request.RequestUri.Should().Be( new Uri( "http://localhost/api/v1/test" ) );
    }

    [Fact]
    public void write_should_do_nothing_when_token_is_absent()
    {
        // arrange
        var request = new HttpRequestMessage( HttpMethod.Get, "http://tempuri.org" );
        var writer = new UrlSegmentApiVersionWriter( "{ver}" );

        // act
        writer.Write( request, new ApiVersion( 1 ) );

        // assert
        request.RequestUri.Should().Be( new Uri( "http://tempuri.org" ) );
    }
}