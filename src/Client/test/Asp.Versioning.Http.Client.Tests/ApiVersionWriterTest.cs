// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.Http;

public class ApiVersionWriterTest
{
    [Fact]
    public void write_should_use_multiple_writers()
    {
        // arrange
        var request = new HttpRequestMessage( HttpMethod.Get, "http://tempuri.org" );
        var writer = ApiVersionWriter.Combine(
            new QueryStringApiVersionWriter( "api-version" ),
            new HeaderApiVersionWriter( "x-ms-version" ) );

        // act
        writer.Write( request, new ApiVersion( 1.0 ) );

        // assert
        request.RequestUri.Should().Be( new Uri( "http://tempuri.org?api-version=1.0" ) );
        request.Headers.GetValues( "x-ms-version" ).Single().Should().Be( "1.0" );
    }

    [Fact]
    public void combine_should_not_allow_empty_sequence()
    {
        // arrange


        // act
        Func<IApiVersionWriter> combine = () => ApiVersionWriter.Combine( Enumerable.Empty<IApiVersionWriter>() );

        // assert
        combine.Should().Throw<ArgumentException>().And.ParamName.Should().Be( "apiVersionWriters" );
    }
}